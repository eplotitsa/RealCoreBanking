using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Account;

[IgnoreAntiforgeryToken]
public class DetailsModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public Core.Models.CurrentAccount? Account { get; private set; }
    public Core.Models.Product?        Product { get; private set; }
    public IReadOnlyList<string> Events { get; private set; } = Array.Empty<string>();
    public string? Message { get; private set; }

    public DetailsModel(AccountStore store) => _store = store;

    public IActionResult OnGet(string? msg = null)
    {
        Account = _store.Get(Id);
        if (Account == null) return NotFound();
        Product = _store.GetProduct(Account.ProductCode);
        Events  = _store.RecentEvents;
        Message = msg;

        // Repair old accounts where BalanceTypePrefix was stored as 0 (pre-migration).
        // Without this, b.Prefix falls back to address/100_000_000 which returns 0 for
        // small addresses, breaking the OTB formula in WithdrawSmartContract.
        if (RepairBalancePrefixes(Account)) _store.SaveAccount(Account);

        return Page();
    }

    private bool RepairBalancePrefixes(Core.Models.CurrentAccount account)
    {
        bool changed = false;
        foreach (var b in account.Balances.Items.Where(x => x.BalanceTypePrefix == 0))
        {
            int pfx = 0;
            // Try product balance code first ("01001"[..2] = "01" → 1)
            if (Product != null)
            {
                var pb = Product.ProductBalances.FirstOrDefault(x => x.BalanceAddress == b.Address);
                if (pb != null && pb.BalanceCode.Length >= 2)
                    int.TryParse(pb.BalanceCode[..2], out pfx);

                // If code gave nonsense (>6), fall back to BalanceDefinition
                if (pfx > 6 || pfx == 0)
                {
                    var def = pb != null ? _store.GetBalanceDefinition(pb.BalanceDefinitionId) : null;
                    if (def != null && def.BalanceTypePrefix > 0)
                        pfx = def.BalanceTypePrefix;
                }
            }
            if (pfx > 0)
            {
                b.BalanceTypePrefix = pfx;
                changed = true;
            }
        }
        return changed;
    }

    // Returns the effective BalanceTypePrefix for a balance.
    // Priority: 1) stored on balance (set at account open), 2) ProductBalance.BalanceCode
    // first two digits (the product-specific type), 3) address-based fallback.
    public int GetEffectivePrefix(Core.Models.Balance b)
    {
        if (b.BalanceTypePrefix > 0) return b.BalanceTypePrefix;
        if (Product != null)
        {
            var pb = Product.ProductBalances.FirstOrDefault(x => x.BalanceAddress == b.Address);
            if (pb != null && pb.BalanceCode.Length >= 2 &&
                int.TryParse(pb.BalanceCode[..2], out var pfx))
                return pfx;
        }
        return (int)(b.Address / 100_000_000L);
    }

    public string GetBalanceRate(long address)
    {
        if (Product == null) return "";
        var entry = Product.Rates.FirstOrDefault(r => r.AccrualBaseAddress == address);
        if (entry == null) return "";
        var rate = _store.GetInterestRate(entry.Code);
        return rate != null ? $"{rate.CurrentRate:F2}%" : "";
    }

    public IActionResult OnPostDeposit(string id, decimal amount, long balanceAddress)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        var ok = a.Deposit(amount, balanceAddress);
        _store.SaveAccount(a);
        return RedirectToPage(new { id, msg = ok ? null : "Пополнение заблокировано" });
    }

    public async Task<IActionResult> OnPostWithdrawAsync(string id, decimal amount, long balanceAddress)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        var ok = await a.WithdrawAsync(amount, balanceAddress);
        _store.SaveAccount(a);
        return RedirectToPage(new { id, msg = ok ? null : "Недостаточно средств или операция заблокирована" });
    }

    public IActionResult OnPostChangeStatus(string id, string newStatus)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        a.ChangeStatus(AccountStatus.Register(newStatus));
        _store.SaveAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleCredit(string id)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        if (a.CreditBlocked) a.UnblockCredit(); else a.BlockCredit();
        _store.SaveAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleDebit(string id)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        if (a.DebitBlocked) a.UnblockDebit(); else a.BlockDebit();
        _store.SaveAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostClose(string id)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        a.Close();
        _store.SaveAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostSetOverdraftLimit(string id, decimal limit)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        var safeLimit = limit < 0 ? 0 : limit;
        a.OverdraftLimit = safeLimit;
        // Sync the overdraft-limit balance (prefix 6) so it reflects the new limit amount.
        // Re-load product to resolve effective prefix for old accounts (BalanceTypePrefix may be 0).
        var prod  = _store.GetProduct(a.ProductCode);
        var odBal = a.Balances.Items.FirstOrDefault(b =>
        {
            if (b.BalanceTypePrefix == 6) return true;
            if (prod != null)
            {
                var pb = prod.ProductBalances.FirstOrDefault(x => x.BalanceAddress == b.Address);
                if (pb != null && pb.BalanceCode.Length >= 2 &&
                    int.TryParse(pb.BalanceCode[..2], out var pfx) && pfx == 6)
                    return true;
            }
            return (int)(b.Address / 100_000_000L) == 6;
        });
        if (odBal != null)
        {
            odBal.Amount = safeLimit;
            if (odBal.BalanceTypePrefix == 0) odBal.BalanceTypePrefix = 6; // fix old record in place
        }
        _store.SaveAccount(a);
        return RedirectToPage(new { id });
    }
}
