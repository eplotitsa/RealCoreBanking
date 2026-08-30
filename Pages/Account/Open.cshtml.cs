using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Account;

public class OpenModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty] public string AccountNumber { get; set; } = "000000001";
    [BindProperty] public string BookType { get; set; } = "MAIN";
    [BindProperty] public BalanceAccountType BalanceAccountType { get; set; }
    [BindProperty] public int ProductCode { get; set; } = 1;
    [BindProperty] public string? CustomerIds { get; set; } = "";
    [BindProperty] public List<BalanceInput> Balances { get; set; } = new();

    public string? Error { get; private set; }
    public IReadOnlyCollection<Product> Products => _store.AllProducts;

    public OpenModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(AccountNumber))
        {
            Error = "Укажите номер счета.";
            return Page();
        }

        var product  = _store.GetProduct(ProductCode);
        var balances = new List<Balance>();

        // If product has defined balances — instantiate them with precision from currency
        if (product != null && product.ProductBalances.Count > 0)
        {
            foreach (var pb in product.ProductBalances)
            {
                var currency = _store.GetCurrency(pb.CurrencyCode);
                var def      = _store.GetBalanceDefinition(pb.BalanceDefinitionId);
                balances.Add(new Balance
                {
                    Address                = pb.BalanceAddress,
                    Name                   = pb.BalanceName,
                    Currency               = pb.CurrencyCode,
                    DecimalPlaces          = currency?.DecimalPlaces ?? 2,
                    RoundingRule           = currency?.RoundingRule ?? RoundingRule.AwayFromZero,
                    FinancialIndicatorCode = def?.FinancialIndicatorCode ?? "001",
                    GlCode                 = pb.GlCode,
                    Amount                 = 0,
                });
            }
        }
        else
        {
            // Fallback: manual balance inputs (for products without configured balances)
            foreach (var b in Balances)
            {
                if (!long.TryParse(b.Address, out long addr) || b.Address.Length != 10)
                {
                    Error = $"Адрес «{b.Address}» должен быть 10-значным числом.";
                    return Page();
                }
                Enum.TryParse<RoundingRule>(b.Rounding, out var rounding);
                balances.Add(new Balance
                {
                    Address                = addr,
                    Name                   = b.Name,
                    Currency               = (b.Currency ?? "USD").ToUpper(),
                    DecimalPlaces          = b.DecimalPlaces,
                    RoundingRule           = rounding,
                    FinancialIndicatorCode = b.FICode ?? "001",
                });
            }

            if (balances.GroupBy(b => (b.Address, b.Currency)).Any(g => g.Count() > 1))
            {
                Error = "Адреса балансов должны быть уникальными в пределах одной валюты.";
                return Page();
            }
        }

        var customerList = (CustomerIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var account = CurrentAccount.Open(
            accountNumber:      AccountNumber.Trim(),
            bookType:           BookType.Trim(),
            balanceAccountType: BalanceAccountType,
            productCode:        ProductCode,
            customerIds:        customerList,
            balances:           balances);

        _store.Add(account);
        return RedirectToPage("/Account/Details", new { id = account.AccountId });
    }

    public class BalanceInput
    {
        public string Address { get; set; } = "0100000001";
        public string Name { get; set; } = "Main";
        public string FICode { get; set; } = "001";
        public string Currency { get; set; } = "USD";
        public int DecimalPlaces { get; set; } = 2;
        public string Rounding { get; set; } = "AwayFromZero";
    }
}
