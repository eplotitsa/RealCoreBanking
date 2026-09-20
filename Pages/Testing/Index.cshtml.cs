using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Testing;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public IndexModel(AccountStore store) => _store = store;

    public CurrentAccount? Account =>
        _store.TestSession.AccountId != null ? _store.Get(_store.TestSession.AccountId) : null;

    public Product? Product =>
        Account != null ? _store.GetProduct(Account.ProductCode) : null;

    public IReadOnlyCollection<CurrentAccount> AllAccounts => _store.All;
    public TestSession Session => _store.TestSession;
    public DateTime BankingDate => _store.Enterprise.BankingDate;
    public List<string> RunLog { get; } = new();

    public void OnGet() { }

    // ── Account selection ───────────────────────────────────────────────────
    public IActionResult OnPostSelectAccount(string accountId)
    {
        _store.TestSession.AccountId = string.IsNullOrEmpty(accountId) ? null : accountId;
        return RedirectToPage();
    }

    // ── Transactions CRUD ───────────────────────────────────────────────────
    public IActionResult OnPostAddTransaction(DateTime txDate, long balanceAddress, decimal amount, string currency)
    {
        _store.TestSession.Transactions.Add(new TestTransaction
        {
            Date           = txDate,
            BalanceAddress = balanceAddress,
            Amount         = amount,
            Currency       = (currency ?? "").ToUpper().Trim(),
        });
        return RedirectToPage();
    }

    public IActionResult OnPostEditTransaction(int txIndex, DateTime txDate, long balanceAddress, decimal amount, string currency)
    {
        var txs = _store.TestSession.Transactions;
        if (txIndex >= 0 && txIndex < txs.Count)
        {
            txs[txIndex].Date           = txDate;
            txs[txIndex].BalanceAddress = balanceAddress;
            txs[txIndex].Amount         = amount;
            txs[txIndex].Currency       = (currency ?? "").ToUpper().Trim();
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteTransaction(int txIndex)
    {
        var txs = _store.TestSession.Transactions;
        if (txIndex >= 0 && txIndex < txs.Count)
            txs.RemoveAt(txIndex);
        return RedirectToPage();
    }

    // ── Run day-close simulation ────────────────────────────────────────────
    public async Task<IActionResult> OnPostRunAsync(DateTime newBankingDay, DateTime? activationDate)
    {
        var account = Account;
        var product = Product;
        if (account == null || product == null)
            return RedirectToPage();

        if (activationDate.HasValue)
            account.ActivationDate = activationDate.Value;

        _store.TestSession.NewBankingDay = newBankingDay;

        var log = await ExecuteDayCloseAsync(account, product, newBankingDay);
        TempData["RunLog"] = System.Text.Json.JsonSerializer.Serialize(log);
        return RedirectToPage();
    }

    public void OnGetShowLog()
    {
        if (TempData.TryGetValue("RunLog", out var raw) && raw is string json)
        {
            var lines = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
            if (lines != null) RunLog.AddRange(lines);
        }
    }

    // ── Day-close logic ─────────────────────────────────────────────────────
    private async Task<List<string>> ExecuteDayCloseAsync(CurrentAccount account, Product product, DateTime newDay)
    {
        var log  = new List<string>();
        var from = _store.Enterprise.BankingDate;

        if (newDay <= from)
        {
            log.Add($"Ошибка: новый банковский день {newDay:dd.MM.yyyy} должен быть позже текущего {from:dd.MM.yyyy}");
            return log;
        }

        for (var day = from.AddDays(1); day <= newDay; day = day.AddDays(1))
        {
            log.Add($"── {day:dd.MM.yyyy} ───────────────────────────────────────");

            // 1. Apply test transactions for this day
            var dayTxs = _store.TestSession.Transactions.Where(t => t.Date.Date == day.Date).ToList();
            foreach (var tx in dayTxs)
            {
                var bal = account.Balances.Items.FirstOrDefault(b => b.Address == tx.BalanceAddress);
                if (bal == null)
                {
                    log.Add($"  [TX] адрес {tx.BalanceAddress:D10} — баланс не найден");
                    continue;
                }
                if (tx.Amount >= 0)
                {
                    var ok = account.Deposit(tx.Amount, tx.BalanceAddress, day);
                    log.Add(ok
                        ? $"  [TX] +{tx.Amount:N2} {tx.Currency} → {tx.BalanceAddress:D10}"
                        : $"  [TX] +{tx.Amount:N2} {tx.Currency} → зачисление заблокировано ({tx.BalanceAddress:D10})");
                }
                else
                {
                    var ok = await account.WithdrawAsync(-tx.Amount, tx.BalanceAddress, day);
                    log.Add(ok
                        ? $"  [TX] {tx.Amount:N2} {tx.Currency} → {tx.BalanceAddress:D10}"
                        : $"  [TX] {tx.Amount:N2} {tx.Currency} → недостаточно средств / ОТВ ({tx.BalanceAddress:D10})");
                }
            }
            if (!dayTxs.Any())
                log.Add("  [TX] транзакций нет");

            // 2. Interest accrual for this day
            bool anyAccrual = false;
            foreach (var rateEntry in product.Rates)
            {
                var rate = _store.GetInterestRate(rateEntry.Code);
                if (rate == null) continue;

                var mainBal = account.Balances.Items.FirstOrDefault(b => b.Address == rateEntry.AccrualBaseAddress);
                if (mainBal == null)
                {
                    log.Add($"  [INT] баланс {rateEntry.AccrualBaseAddress:D10} не найден (ставка {rateEntry.Code})");
                    continue;
                }
                if (mainBal.Amount == 0) continue;

                var interest = rate.CalculateInterest(mainBal.Amount, day, day.AddDays(1), product.AccrualBasis);
                if (interest <= 0) continue;

                anyAccrual = true;
                var accrBal = account.Balances.Items.FirstOrDefault(b => b.Address == rateEntry.AccrualTargetAddress);
                if (accrBal != null)
                {
                    accrBal.Add(interest);
                    log.Add($"  [INT] {interest:N10} {mainBal.Currency} (ставка {rate.CurrentRate}%) → {accrBal.Address:D10}");
                }
                else
                {
                    log.Add($"  [INT] {interest:N10} {mainBal.Currency} (ставка {rate.CurrentRate}%) — целевой баланс {rateEntry.AccrualTargetAddress:D10} не найден");
                }
            }
            if (!anyAccrual && product.Rates.Any())
                log.Add("  [INT] нулевой баланс, начисление не требуется");
            else if (!product.Rates.Any())
                log.Add("  [INT] ставки не назначены продукту");
        }

        _store.Enterprise.SetDate(newDay);
        _store.SaveAccount(account);
        log.Add($"══ Банковская дата установлена: {newDay:dd.MM.yyyy} ══");
        return log;
    }
}
