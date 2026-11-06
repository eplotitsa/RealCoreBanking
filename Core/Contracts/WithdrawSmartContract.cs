using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Core.Contracts;

/// <summary>
/// Two-phase withdrawal smart contract.
///
/// Phase 1 — Initial posting (Dr main balance / Cr external source):
///   The full amount is debited from the main balance. If the balance goes negative,
///   Phase 2 will rebalance via the overdraft account.
///
/// Phase 2 — AfterPosting rebalance (Dr main balance / Cr overdraft-used):
///   If the main balance is negative after Phase 1 the contract fires an additional
///   posting with the same TxId to zero the main balance and record the overdraft usage.
///
/// Pre-condition: OTB check (prefix-1 − prefix-2 − prefix-4 − prefix-5 + prefix-6 ≥ amount).
/// </summary>
public class WithdrawSmartContract : ISmartContract
{
    private readonly decimal  _amount;
    private readonly long     _balanceAddress;
    private readonly DateTime _date;

    public string Name    => "СК: Списание с учётом ОТВ и овердрафта";
    public bool   Success { get; private set; }

    public WithdrawSmartContract(decimal amount, long balanceAddress, DateTime? date = null)
    {
        _amount         = amount;
        _balanceAddress = balanceAddress;
        _date           = date ?? DateTime.UtcNow;
    }

    public Task ExecuteAsync(CurrentAccount account, CancellationToken ct = default)
    {
        var source = account.Balances.GetByAddress(_balanceAddress);
        if (source == null) { Success = false; return Task.CompletedTask; }

        var items = account.Balances.Items
            .Where(b => b.Currency.Equals(source.Currency, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // ── Pre-flight OTB check ─────────────────────────────────────────────
        var otb = items.Where(b => b.Prefix == 1).Sum(b => b.Amount)
                - items.Where(b => b.Prefix == 2).Sum(b => b.Amount)
                - items.Where(b => b.Prefix == 4).Sum(b => b.Amount)
                - items.Where(b => b.Prefix == 5).Sum(b => b.Amount)
                + items.Where(b => b.Prefix == 6).Sum(b => b.Amount);

        if (_amount > otb) { Success = false; return Task.CompletedTask; }

        // ── Pre-flight overdraft-account check ───────────────────────────────
        // If the withdrawal exceeds the main balance, the deficit must be recorded on the
        // overdraft-used balance (prefix 5). Without it the deficit would silently vanish.
        if (_amount > source.Amount && !items.Any(b => b.Prefix == 5))
        {
            Success = false;
            return Task.CompletedTask;
        }

        var txId = "TX" + _date.ToString("yyyyMMdd") + "-" +
                   Guid.NewGuid().ToString("N")[..6].ToUpper();

        var tx = new AccountTransaction
        {
            TxId           = txId,
            Date           = _date,
            Amount         = _amount,
            Currency       = source.Currency,
            BalanceAddress = _balanceAddress,
            Type           = "Financial",
            Status         = "Posted",
            Description    = "Withdrawal",
        };

        // ── Phase 1: Dr main balance / Cr external source ───────────────────
        // Allow balance to go negative — Phase 2 will rebalance via overdraft.
        source.Amount -= _amount;

        tx.Postings.Add(new Posting
        {
            PostingId   = txId + "-P1",
            TxId        = txId,
            Date        = _date,
            DrAddress   = _balanceAddress, // Dr main balance
            CrAddress   = 0,               // Cr external source
            Amount      = _amount,
            Currency    = source.Currency,
            Description = "Dr main balance / Cr external source",
        });

        // ── Phase 2 (AfterPosting): rebalance via overdraft if balance < 0 ───
        if (source.Amount < 0)
        {
            var deficit = Math.Abs(source.Amount);
            var odUsed  = items.FirstOrDefault(b => b.Prefix == 5);

            source.Amount = 0;
            if (odUsed != null) odUsed.Add(deficit);
            // odLimit (prefix=6) stores the fixed approved credit limit — never reduced here.
            // OTB = prefix1 + prefix6 − prefix5 accounts for usage via prefix5 growth.

            tx.Postings.Add(new Posting
            {
                PostingId   = txId + "-P2",
                TxId        = txId,
                Date        = _date,
                DrAddress   = _balanceAddress,       // Dr main balance (zeroing it)
                CrAddress   = odUsed?.Address ?? 0,  // Cr overdraft-used
                Amount      = deficit,
                Currency    = source.Currency,
                Description = "AfterPosting: Dr main balance / Cr overdraft-used",
            });
        }

        account.Transactions.Add(tx);
        Success = true;
        return Task.CompletedTask;
    }
}
