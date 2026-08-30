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
    public DateTime? DueDate { get; set; }
    public bool CreditBlocked { get; set; }
    public bool DebitBlocked { get; set; }
    public decimal OverdraftLimit { get; set; }
    public BalancesCollection Balances { get; set; } = new();
    public HoldsList HoldsList { get; set; } = new();
    public DebtArray DebtArray { get; set; } = new();
    public List<StandingOrder> StandingOrdersOutgoing { get; set; } = new();
    public List<StandingOrder> StandingOrdersIncoming { get; set; } = new();

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

    public bool Deposit(decimal amount, long balanceAddress)
    {
        if (CreditBlocked) return false;
        if (StatusValue == AccountStatus.Closed.Value) return false;
        var balance = Balances.GetByAddress(balanceAddress);
        if (balance == null) return false;
        balance.Add(amount);
        LastTransactionDate = DateTime.UtcNow;
        EventBus.Publish(new DepositEvent(AccountNumber, amount, balanceAddress));
        return true;
    }

    public bool Withdraw(decimal amount, long balanceAddress)
    {
        if (DebitBlocked) return false;
        if (StatusValue == AccountStatus.Closed.Value) return false;
        var balance = Balances.GetByAddress(balanceAddress);
        if (balance == null) return false;
        if (!balance.Subtract(amount)) return false;
        LastTransactionDate = DateTime.UtcNow;
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
