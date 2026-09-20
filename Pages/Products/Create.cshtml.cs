using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Products;

public class CreateModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty] public int  Code { get; set; }
    [BindProperty] public string Name     { get; set; } = "";
    [BindProperty] public string Category { get; set; } = "Client";
    [BindProperty] public string AccrualBasis { get; set; } = "ACT/ACT";
    [BindProperty] public BalanceAccountType BalanceType { get; set; } = BalanceAccountType.Passive;
    [BindProperty] public decimal DefaultOverdraftLimit  { get; set; }
    [BindProperty] public bool SupportsStandingOrders       { get; set; }
    [BindProperty] public bool SupportsDirectDebit          { get; set; }
    [BindProperty] public bool SupportsAuthorizationHolds   { get; set; }
    [BindProperty] public bool SupportsPendingObligationQueue { get; set; }

    [BindProperty] public List<ProductBalanceInput>   Balances             { get; set; } = new();
    [BindProperty] public List<GlCodeEntry>           GlCodes              { get; set; } = new();
    [BindProperty] public List<DebtBalanceRef>        DebtBalanceAddresses { get; set; } = new();
    [BindProperty] public List<RateInputRow>          Rates                { get; set; } = new();
    [BindProperty] public List<DictEntryRow>          DictEntries          { get; set; } = new();

    public string? Error { get; set; }

    public IReadOnlyCollection<BalanceDefinition> BalanceDefs => _store.AllBalanceDefinitions;
    public IReadOnlyCollection<Currency>          Currencies  => _store.AllCurrencies;

    public static readonly string[] AccrualBases = { "ACT/ACT", "ACT/360", "ACT/365F", "30/360" };

    public CreateModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (Code <= 0)             { Error = "Код продукта должен быть больше 0.";  return Page(); }
        if (string.IsNullOrWhiteSpace(Name)) { Error = "Введите название продукта."; return Page(); }
        if (_store.GetProduct(Code) != null) { Error = $"Продукт с кодом {Code} уже существует."; return Page(); }

        var productBalances = new List<ProductBalance>();
        foreach (var b in Balances.Where(b => !string.IsNullOrWhiteSpace(b.BalanceDefinitionId) && b.BalanceAddress > 0))
        {
            var def = _store.GetBalanceDefinition(b.BalanceDefinitionId);
            if (def == null) continue;
            productBalances.Add(new ProductBalance
            {
                BalanceDefinitionId = def.Id,
                BalanceCode         = def.Code,
                BalanceName         = b.BalanceName.Trim(),
                CurrencyCode        = (b.CurrencyCode ?? "USD").ToUpper().Trim(),
                BalanceAddress      = b.BalanceAddress,
                GlCode              = b.GlCode?.Trim() ?? "",
            });
        }

        var doi = new SortedDictionary<decimal, decimal>();
        foreach (var d in DictEntries.Where(d => d.Threshold > 0))
            doi[d.Threshold] = d.Modifier;

        var product = new Product
        {
            Code             = Code,
            Name             = Name.Trim(),
            Category         = Category,
            AccrualBasis     = AccrualBasis,
            BalanceType      = BalanceType,
            DefaultOverdraftLimit          = DefaultOverdraftLimit,
            SupportsStandingOrders         = SupportsStandingOrders,
            SupportsDirectDebit            = SupportsDirectDebit,
            SupportsAuthorizationHolds     = SupportsAuthorizationHolds,
            SupportsPendingObligationQueue = SupportsPendingObligationQueue,
            ProductBalances      = productBalances,
            DictionaryOfInterest = doi,
            GlCodes = GlCodes
                .Where(g => !string.IsNullOrWhiteSpace(g.GlCode) && !string.IsNullOrWhiteSpace(g.Currency))
                .ToList(),
            DebtBalanceAddresses = DebtBalanceAddresses
                .Where(d => d.Address > 0 && !string.IsNullOrWhiteSpace(d.Currency))
                .ToList(),
            Rates = Rates
                .Where(r => r.Code > 0)
                .Select(r => new RateEntry
                {
                    Code                 = r.Code,
                    Name                 = r.Name ?? "",
                    AccrualBaseAddress   = r.AccrualBaseAddress,
                    AccrualTargetAddress = r.AccrualTargetAddress,
                })
                .ToList()
        };

        _store.AddProduct(product);
        return RedirectToPage("/Products/Details", new { code = product.Code });
    }
}

public class ProductBalanceInput
{
    public string BalanceDefinitionId { get; set; } = "";
    public string BalanceName         { get; set; } = "";
    public string CurrencyCode        { get; set; } = "USD";
    public long   BalanceAddress      { get; set; }
    public string GlCode              { get; set; } = "";
}

public class RateInputRow
{
    public int    Code                 { get; set; }
    public string Name                 { get; set; } = "";
    public long   AccrualBaseAddress   { get; set; }
    public long   AccrualTargetAddress { get; set; }
}

public class DictEntryRow
{
    public decimal Threshold { get; set; }
    public decimal Modifier  { get; set; }
}
