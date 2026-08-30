using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Currencies;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public IReadOnlyCollection<Currency> Currencies => _store.AllCurrencies;
    public string? Message { get; private set; }

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPostSave(
        string? existingAlphaCode,
        string alphaCode, string numericCode, string name,
        int decimalPlaces, RoundingRule roundingRule)
    {
        alphaCode   = alphaCode.Trim().ToUpper();
        numericCode = numericCode.Trim();
        name        = name.Trim();

        if (string.IsNullOrWhiteSpace(alphaCode) || alphaCode.Length != 3)
        { Message = "Alpha-код должен быть ровно 3 символа (ISO 4217)."; return Page(); }
        if (string.IsNullOrWhiteSpace(numericCode))
        { Message = "Цифровой код обязателен."; return Page(); }
        if (string.IsNullOrWhiteSpace(name))
        { Message = "Название обязательно."; return Page(); }

        // If alpha code changed — remove old entry
        if (!string.IsNullOrEmpty(existingAlphaCode) &&
            !existingAlphaCode.Equals(alphaCode, StringComparison.OrdinalIgnoreCase))
        {
            _store.DeleteCurrency(existingAlphaCode);
        }

        _store.SaveCurrency(new Currency
        {
            AlphaCode    = alphaCode,
            NumericCode  = numericCode,
            Name         = name,
            DecimalPlaces= decimalPlaces,
            RoundingRule = roundingRule,
        });

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(string alphaCode)
    {
        _store.DeleteCurrency(alphaCode);
        return RedirectToPage();
    }
}
