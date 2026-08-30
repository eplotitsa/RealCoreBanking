using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Balances;

public class CreateModel : PageModel
{
    private readonly AccountStore _store;

    public IReadOnlyCollection<CurrentAccount> Accounts => _store.All;
    public string? ErrorMessage { get; private set; }

    // Pre-selected account when navigating from account page
    [BindProperty(SupportsGet = true)]
    public string? AccountId { get; set; }

    public CreateModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPost(string accountId, long address, string name, string currency,
        string financialIndicatorCode, int decimalPlaces, RoundingRule roundingRule)
    {
        var account = _store.Get(accountId);
        if (account == null)
        {
            ErrorMessage = "Счет не найден.";
            return Page();
        }

        if (account.Balances.GetByAddress(address) != null)
        {
            ErrorMessage = $"Баланс с адресом {address:D10} уже существует в счёте {account.AccountNumber}.";
            AccountId = accountId;
            return Page();
        }

        account.Balances.Add(new Balance
        {
            Address                = address,
            Name                   = name.Trim(),
            Currency               = currency.ToUpper().Trim(),
            FinancialIndicatorCode = financialIndicatorCode,
            DecimalPlaces          = decimalPlaces,
            RoundingRule           = roundingRule,
        });

        return RedirectToPage("/Balances/Index");
    }
}
