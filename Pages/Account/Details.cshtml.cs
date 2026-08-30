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
    public IReadOnlyList<string> Events { get; private set; } = Array.Empty<string>();
    public string? Message { get; private set; }

    public DetailsModel(AccountStore store) => _store = store;

    public IActionResult OnGet(string? msg = null)
    {
        Account = _store.Get(Id);
        if (Account == null) return NotFound();
        Events = _store.RecentEvents;
        Message = msg;
        return Page();
    }

    public IActionResult OnPostDeposit(string id, decimal amount, long balanceAddress)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        var ok = a.Deposit(amount, balanceAddress);
        return RedirectToPage(new { id, msg = ok ? null : "Пополнение заблокировано" });
    }

    public IActionResult OnPostWithdraw(string id, decimal amount, long balanceAddress)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        var ok = a.Withdraw(amount, balanceAddress);
        return RedirectToPage(new { id, msg = ok ? null : "Недостаточно средств или операция заблокирована" });
    }

    public IActionResult OnPostChangeStatus(string id, string newStatus)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        a.ChangeStatus(AccountStatus.Register(newStatus));
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleCredit(string id)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        if (a.CreditBlocked) a.UnblockCredit(); else a.BlockCredit();
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostToggleDebit(string id)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        if (a.DebitBlocked) a.UnblockDebit(); else a.BlockDebit();
        return RedirectToPage(new { id });
    }

    public IActionResult OnPostClose(string id)
    {
        var a = _store.Get(id);
        if (a == null) return NotFound();
        a.Close();
        return RedirectToPage(new { id });
    }
}
