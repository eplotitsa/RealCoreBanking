using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealCoreBanking.Core.Models;

public class CurrentAccount
{
    public string AccountId { get; set; } = Guid.NewGuid().ToString();
    public string AccountNumber { get; set; } = "";
    public string BookType { get; set; } = "";
    public BalanceAccountType BalanceAccountType { get; set; }
    public int ProductCode { get; set; }
    public List<string> CustomerIds { get; set; } = new();
    public string StatusValue { get; set; } = "open";
    public DateTime OpenedDate { get; set; }
    public DateTime? ActivationDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public DateTime? BillingDate { get; set; }
    public int GracePeriodDays { get; set; }
    public DateTime? DueDate { get; set; }
    public bool CreditBlocked { get; set; }
    public bool DebitBlocked { get; set; }
    public decimal OverdraftLimit { get; set; }
    public BalancesCollection Balances { get; set; } = new();
    public HoldsList HoldsList { get; set; } = new();
    public DebtArray DebtArray { get; set; } = new();
    public List<StandingOrder> StandingOrdersOutgoing { get; set; } = new();
    public List<StandingOrder> StandingOrdersIncoming { get; set; } = new();
    public List<AccountTransaction> Transactions { get; set; } = new();

    [JsonIgnore]
    public AccountStatus Status
    {
        get => AccountStatus.FromString(StatusValue) ?? AccountStatus.Open;
        set => StatusValue = value.Value;
    }

    [JsonIgnore]
    public Balance? MainBalance =>
        Balances.Items.FirstOrDefault(b => b.Prefix == 1 && b.FinancialIndicatorCode == "001");

    public static CurrentAccount Open(
        string accountNumber,
        string bookType,
        BalanceAccountType balanceAccountType,
        int productCode,
        List<string>? customerIds,
        IEnumerable<Balance> balances)
    {
        var col = new BalancesCollection();
        foreach (var b in balances)
            col.Add(b);

        var now = DateTime.UtcNow;
        var account = new CurrentAccount
        {
            AccountNumber      = accountNumber,
            BookType           = bookType,
            BalanceAccountType = balanceAccountType,
            ProductCode        = productCode,
            CustomerIds        = customerIds ?? new List<string>(),
            Balances           = col,
            OpenedDate         = now,
            ActivationDate     = now,   // initial status is "open"
            BillingDate        = now,
            DueDate            = now,
        };

        EventBus.Publish(new AccountOpenedEvent(accountNumber));
        return account;
    }

    public void Close()
    {
        var old = StatusValue;
        StatusValue = AccountStatus.Closed.Value;
        EventBus.Publish(new AccountClosedEvent(AccountNumber));
        EventBus.Publish(new AccountStatusChangedEvent(AccountNumber, old, StatusValue));
    }

    public void BlockCredit()
    {
        CreditBlocked = true;
        EventBus.Publish(new CreditBlockedEvent(AccountNumber));
    }

    public void UnblockCredit() => CreditBlocked = false;

    public void BlockDebit()
    {
        DebitBlocked = true;
        EventBus.Publish(new DebitBlockedEvent(AccountNumber));
    }

    public void UnblockDebit() => DebitBlocked = false;

    public void ChangeStatus(AccountStatus newStatus)
    {
        var old = StatusValue;
        StatusValue = newStatus.Value;
        if (newStatus == AccountStatus.Open)
            ActivationDate = DateTime.UtcNow;
        EventBus.Publish(new AccountStatusChangedEvent(AccountNumber, old, StatusValue));
    }

    public void ChangeProduct(int newProductCode)
    {
        ProductCode = newProductCode;
        EventBus.Publish(new ProductChangedEvent(AccountNumber, ProductCode.ToString(), newProductCode.ToString()));
    }

    public bool Deposit(decimal amount, long balanceAddress, DateTime? date = null)
    {
        if (CreditBlocked) return false;
        if (StatusValue == AccountStatus.Closed.Value) return false;
        var balance = Balances.GetByAddress(balanceAddress);
        if (balance == null) return false;

        var txDate = date ?? DateTime.UtcNow;
        var txId   = "TX" + txDate.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Phase 1: Dr external source / Cr main balance
        balance.Add(amount);

        var tx = new AccountTransaction
        {
            TxId           = txId,
            Date           = txDate,
            Amount         = amount,
            Currency       = balance.Currency,
            BalanceAddress = balanceAddress,
            Type           = "Financial",
            Status         = "Posted",
            Description    = "Deposit",
        };
        tx.Postings.Add(new Posting
        {
            PostingId   = txId + "-P1",
            TxId        = txId,
            Date        = txDate,
            DrAddress   = 0,             // Dr external source
            CrAddress   = balanceAddress, // Cr main balance
            Amount      = amount,
            Currency    = balance.Currency,
            Description = "Dr external source / Cr main balance",
        });

        // AfterPosting: if there is used overdraft — repay it from the deposited funds
        var odUsed = Balances.Items.FirstOrDefault(b =>
            b.Prefix == 5 &&
            b.Currency.Equals(balance.Currency, StringComparison.OrdinalIgnoreCase) &&
            b.Amount > 0);
        if (odUsed != null)
        {
            var repay = Math.Min(balance.Amount, odUsed.Amount);
            balance.Amount -= repay;
            odUsed.Amount  -= repay;
            tx.Postings.Add(new Posting
            {
                PostingId   = txId + "-P2",
                TxId        = txId,
                Date        = txDate,
                DrAddress   = balanceAddress, // Dr main balance
                CrAddress   = odUsed.Address, // Cr overdraft-used (repayment)
                Amount      = repay,
                Currency    = balance.Currency,
                Description = "AfterPosting: Dr main balance / Cr overdraft-used (repayment)",
            });
        }

        Transactions.Add(tx);
        LastTransactionDate = txDate;
        EventBus.Publish(new DepositEvent(AccountNumber, amount, balanceAddress));
        return true;
    }

    public async Task<bool> WithdrawAsync(decimal amount, long balanceAddress, DateTime? date = null, CancellationToken ct = default)
    {
        if (DebitBlocked) return false;
        if (StatusValue == AccountStatus.Closed.Value) return false;

        var contract = new RealCoreBanking.Core.Contracts.WithdrawSmartContract(amount, balanceAddress, date);
        var pipeline = new RealCoreBanking.Core.Contracts.SmartContractPipeline();
        await pipeline.RunAsync(this, [contract], ct);
        if (!contract.Success) return false;

        LastTransactionDate = date ?? DateTime.UtcNow;
        EventBus.Publish(new WithdrawEvent(AccountNumber, amount, balanceAddress));
        return true;
    }

    // AvailableAmount = Σ(prefix=01, fi=001) - Σ(prefix=02) - Σ(prefix=04) + OverdraftLimit
    public decimal GetAvailableBalance() => Balances.AvailableAmount + OverdraftLimit - HoldsList.TotalAmount;

    public void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(this, options));
    }

    public static CurrentAccount? LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<CurrentAccount>(json);
    }
}
