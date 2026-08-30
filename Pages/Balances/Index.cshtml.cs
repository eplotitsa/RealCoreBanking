using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Balances;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public IReadOnlyCollection<BalanceDefinition> Definitions => _store.AllBalanceDefinitions;
    public string? Message { get; private set; }

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPostSaveDefinition(
        string? id,
        string name,
        int balanceTypePrefix,
        string financialIndicatorCode)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Message = "Название обязательно.";
            return Page();
        }

        var def = (!string.IsNullOrEmpty(id) ? _store.GetBalanceDefinition(id) : null)
                  ?? new BalanceDefinition();

        def.Name                   = name.Trim();
        def.BalanceTypePrefix      = balanceTypePrefix;
        def.FinancialIndicatorCode = financialIndicatorCode;

        _store.AddBalanceDefinition(def);

        return RedirectToPage();
    }

    public IActionResult OnPostDeleteDefinition(string id)
    {
        _store.DeleteBalanceDefinition(id);
        return RedirectToPage();
    }
}
