using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InterestRates;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public List<InterestRate> Rates   { get; set; } = new();
    public List<RateIndex>    Indexes { get; set; } = new();

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet()
    {
        Rates   = _store.AllRates.OrderBy(r => r.Code).ToList();
        Indexes = _store.AllIndexes.OrderBy(i => i.Code).ToList();
    }

    public IActionResult OnPostDeleteRate(int code)
    {
        _store.DeleteInterestRate(code);
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteIndex(string code)
    {
        _store.DeleteRateIndex(code);
        return RedirectToPage();
    }
}
