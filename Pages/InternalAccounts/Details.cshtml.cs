using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;


namespace RealCoreBanking.Pages.InternalAccounts;

[IgnoreAntiforgeryToken]
public class DetailsModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public InternalAccount? Account { get; private set; }
    public string?          Message { get; private set; }

    public DetailsModel(AccountStore store) => _store = store;

    public IActionResult OnGet(string? msg = null)
    {
        Account = _store.GetInternalAccount(Id);
        if (Account == null) return NotFound();
        Message = msg;
        return Page();
    }

    public int GetEffectivePrefix(Balance b)
    {
        if (b.BalanceTypePrefix > 0) return b.BalanceTypePrefix;
        return (int)(b.Address / 100_000_000L);
    }

    public IActionResult OnPostDeposit(string id, decimal amount, long balanceAddress)
    {
        var a = _store.GetInternalAccount(id);
        if (a == null) return NotFound();
        var ok = a.Deposit(amount, balanceAddress);
        _store.SaveInternalAccount(a);
        return RedirectToPage(new { id, msg = ok ? null : "Пополнение заблокировано" });
    }

    public async Task<IActionResult> OnPostWithdrawAsync(string id, decimal amount, long balanceAddress)
    {
        var a = _store.GetInternalAccount(id);
        if (a == null) return NotFound();
        var ok = await a.WithdrawAsync(amount, balanceAddress);
        _store.SaveInternalAccount(a);
        return RedirectToPage(new { id, msg = ok ? null : "Недостаточно средств или операция заблокирована" });
    }

    public IActionResult OnPostChangeStatus(string id, string newStatus)
    {
        var a = _store.GetInternalAccount(id);
        if (a == null) return NotFound();
        a.ChangeStatus(AccountStatus.Register(newStatus));
        _store.SaveInternalAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleCredit(string id)
    {
        var a = _store.GetInternalAccount(id);
        if (a == null) return NotFound();
        if (a.CreditBlocked) a.UnblockCredit(); else a.BlockCredit();
        _store.SaveInternalAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleDebit(string id)
    {
        var a = _store.GetInternalAccount(id);
        if (a == null) return NotFound();
        if (a.DebitBlocked) a.UnblockDebit(); else a.BlockDebit();
        _store.SaveInternalAccount(a);
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostClose(string id)
    {
        var a = _store.GetInternalAccount(id);
        if (a == null) return NotFound();
        a.Close();
        _store.SaveInternalAccount(a);
        return RedirectToPage(new { id });
    }
}
