using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.BalanceTypes;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public IReadOnlyList<BalanceTypeDefinition> All => BalanceTypeDefinition.All;

    [BindProperty] public string NewCode { get; set; } = "";
    [BindProperty] public string NewDescription { get; set; } = "";

    public string? Message { get; private set; }

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPostAdd()
    {
        NewCode = (NewCode ?? "").Trim().PadLeft(2, '0');
        if (NewCode.Length != 2 || !NewCode.All(char.IsDigit))
        {
            Message = "Код должен быть ровно 2 цифры (например, 06).";
            return Page();
        }
        if (BalanceTypeDefinition.All.Any(t => t.Code == NewCode))
        {
            Message = $"Код «{NewCode}» уже существует.";
            return Page();
        }
        if (string.IsNullOrWhiteSpace(NewDescription))
        {
            Message = "Описание не может быть пустым.";
            return Page();
        }
        _store.SaveBalanceType(NewCode, NewDescription.Trim());
        return RedirectToPage();
    }

    public IActionResult OnPostEdit(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Message = "Описание не может быть пустым.";
            return Page();
        }
        if (!BalanceTypeDefinition.All.Any(t => t.Code == code))
        {
            Message = $"Код «{code}» не найден.";
            return Page();
        }
        _store.SaveBalanceType(code, description.Trim());
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(string code)
    {
        _store.DeleteBalanceType(code);
        return RedirectToPage();
    }
}
