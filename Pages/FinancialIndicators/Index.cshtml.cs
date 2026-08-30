using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.FinancialIndicators;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public IReadOnlyList<FinancialIndicator> All => FinancialIndicator.All;

    [BindProperty] public string NewCode { get; set; } = "";
    [BindProperty] public string NewDescription { get; set; } = "";

    public string? Message { get; private set; }

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPostAdd()
    {
        NewCode = (NewCode ?? "").Trim();
        if (NewCode.Length != 3 || !NewCode.All(char.IsDigit))
        {
            Message = "Код должен быть ровно 3 цифры (например, 010).";
            return Page();
        }
        if (FinancialIndicator.All.Any(f => f.Code == NewCode))
        {
            Message = $"Код «{NewCode}» уже существует.";
            return Page();
        }
        if (string.IsNullOrWhiteSpace(NewDescription))
        {
            Message = "Описание не может быть пустым.";
            return Page();
        }
        _store.SaveFinancialIndicator(NewCode, NewDescription.Trim());
        return RedirectToPage();
    }

    public IActionResult OnPostEdit(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Message = "Описание не может быть пустым.";
            return Page();
        }
        if (!FinancialIndicator.All.Any(f => f.Code == code))
        {
            Message = $"Код «{code}» не найден.";
            return Page();
        }
        _store.SaveFinancialIndicator(code, description.Trim());
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(string code)
    {
        _store.DeleteFinancialIndicator(code);
        return RedirectToPage();
    }
}
