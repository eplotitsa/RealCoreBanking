using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Enterprise;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;
    private readonly IHostApplicationLifetime _lifetime;

    public IndexModel(AccountStore store, IHostApplicationLifetime lifetime)
    {
        _store = store;
        _lifetime = lifetime;
    }

    public RealCoreBanking.Core.Models.Enterprise Enterprise => _store.Enterprise;

    public void OnGet() { }

    public IActionResult OnPostAdvance(int days = 1)
    {
        if (days < 1) days = 1;
        if (days > 365) days = 365;
        _store.Enterprise.AdvanceDate(days);
        return RedirectToPage();
    }

    public IActionResult OnPostSetDate(DateTime bankingDate)
    {
        _store.Enterprise.SetDate(bankingDate);
        return RedirectToPage();
    }

    public IActionResult OnPostStop()
    {
        _lifetime.StopApplication();
        return RedirectToPage();
    }
}
