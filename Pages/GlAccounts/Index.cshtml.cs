using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Data.Repositories;

namespace RealCoreBanking.Pages.GlAccounts;

public class IndexModel : PageModel
{
    private readonly IGlAccountRepository _repo;

    public IReadOnlyList<(string Number, string Name, string BalanceType)> All { get; private set; } = [];

    [BindProperty] public string NewNumber      { get; set; } = "";
    [BindProperty] public string NewName        { get; set; } = "";
    [BindProperty] public string NewBalanceType { get; set; } = "Assets";

    public string? Message { get; private set; }

    public static readonly string[] BalanceTypes = ["Assets", "Liability", "OffBalance"];

    public IndexModel(IGlAccountRepository repo) => _repo = repo;

    public void OnGet() => All = _repo.GetAll();

    public IActionResult OnPostAdd()
    {
        All = _repo.GetAll();
        NewNumber = (NewNumber ?? "").Trim();
        if (string.IsNullOrWhiteSpace(NewNumber))
            { Message = "Номер счета не может быть пустым."; return Page(); }
        if (All.Any(x => x.Number == NewNumber))
            { Message = $"Счет «{NewNumber}» уже существует."; return Page(); }
        if (string.IsNullOrWhiteSpace(NewName))
            { Message = "Название не может быть пустым."; return Page(); }
        if (!BalanceTypes.Contains(NewBalanceType))
            { Message = "Недопустимый тип баланса."; return Page(); }

        _repo.Save(NewNumber, NewName.Trim(), NewBalanceType);
        return RedirectToPage();
    }

    public IActionResult OnPostEdit(string number, string name, string balanceType)
    {
        All = _repo.GetAll();
        if (string.IsNullOrWhiteSpace(name))
            { Message = "Название не может быть пустым."; return Page(); }
        if (!All.Any(x => x.Number == number))
            { Message = $"Счет «{number}» не найден."; return Page(); }
        if (!BalanceTypes.Contains(balanceType))
            { Message = "Недопустимый тип баланса."; return Page(); }

        _repo.Save(number, name.Trim(), balanceType);
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(string number)
    {
        _repo.Delete(number);
        return RedirectToPage();
    }
}
