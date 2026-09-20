using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InternalAccounts;

public class OpenModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty] public string AccountNumber { get; set; } = "";
    [BindProperty] public string BookType      { get; set; } = "Main";
    [BindProperty] public string OwnerId       { get; set; } = "";
    [BindProperty] public string OwnerType     { get; set; } = "Департамент";
    [BindProperty] public List<BalanceInput> Balances { get; set; } = new();

    public string? Error { get; private set; }

    public static readonly string[] OwnerTypes =
        ["Банк", "Филиал", "Департамент", "Служебный процесс", "Другое"];

    public OpenModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(AccountNumber))
        {
            Error = "Укажите номер счёта.";
            return Page();
        }
        if (string.IsNullOrWhiteSpace(OwnerId))
        {
            Error = "Укажите идентификатор владельца.";
            return Page();
        }

        var balances = new List<Balance>();
        foreach (var b in Balances)
        {
            if (!long.TryParse(b.Address, out long addr) || b.Address.Length != 10)
            {
                Error = $"Адрес «{b.Address}» должен быть 10-значным числом.";
                return Page();
            }
            int balTypePfx = (int)(addr / 100_000_000L);
            var currency = _store.GetCurrency(b.Currency.ToUpper());
            Enum.TryParse<RoundingRule>(b.Rounding, out var rounding);
            balances.Add(new Balance
            {
                Address                = addr,
                Name                   = b.Name,
                Currency               = b.Currency.ToUpper(),
                DecimalPlaces          = currency?.DecimalPlaces ?? 2,
                RoundingRule           = currency?.RoundingRule ?? rounding,
                FinancialIndicatorCode = b.FICode.Length > 0 ? b.FICode : "001",
                GlCode                 = b.GlCode,
                BalanceTypePrefix      = balTypePfx,
                Amount                 = 0,
            });
        }

        if (balances.GroupBy(b => (b.Address, b.Currency)).Any(g => g.Count() > 1))
        {
            Error = "Адреса балансов должны быть уникальными.";
            return Page();
        }

        var account = InternalAccount.Open(
            accountNumber: AccountNumber.Trim(),
            bookType:      BookType.Trim(),
            ownerId:       OwnerId.Trim(),
            ownerType:     OwnerType,
            balances:      balances);

        _store.AddInternalAccount(account);
        return RedirectToPage("/InternalAccounts/Details", new { id = account.AccountId });
    }

    public class BalanceInput
    {
        public string Address { get; set; } = "0100000001";
        public string Name    { get; set; } = "";
        public string FICode  { get; set; } = "001";
        public string GlCode  { get; set; } = "";
        public string Currency { get; set; } = "USD";
        public string Rounding { get; set; } = "AwayFromZero";
    }
}
