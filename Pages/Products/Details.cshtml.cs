using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Products;

public class DetailsModel : PageModel
{
    private readonly AccountStore _store;

    public Product?                              Product     { get; set; }
    public List<InterestRate>                    LinkedRates { get; set; } = new();
    public IReadOnlyCollection<Currency>          Currencies    => _store.AllCurrencies;
    public IReadOnlyCollection<InterestRate>      AllRates      => _store.AllRates;
    public IReadOnlyList<(string Number, string Name, string BalanceType)> AllGlAccounts       => _store.AllGlAccounts;
    public IReadOnlyCollection<BalanceDefinition>                         AllBalanceDefinitions => _store.AllBalanceDefinitions;
    public bool                                   EditMode    { get; private set; }

    public DetailsModel(AccountStore store) => _store = store;

    public IActionResult OnGet(int code, bool edit = false)
    {
        Product  = _store.GetProduct(code);
        if (Product == null) return NotFound();
        EditMode = edit;
        LoadLinkedRates();
        return Page();
    }

    // ── Basic fields ────────────────────────────────────────────────────────
    public IActionResult OnPostUpdateBasic(int code, string name, string accrualBasis,
        BalanceAccountType balanceType, bool supportsStandingOrders, bool supportsDirectDebit,
        bool supportsAuthorizationHolds, bool supportsPendingObligationQueue, bool supportsDebtList,
        decimal defaultOverdraftLimit)
    {
        var p = Require(code);
        p.Name                           = name.Trim();
        p.AccrualBasis                   = accrualBasis;
        p.BalanceType                    = balanceType;
        p.SupportsStandingOrders         = supportsStandingOrders;
        p.SupportsDirectDebit            = supportsDirectDebit;
        p.SupportsAuthorizationHolds     = supportsAuthorizationHolds;
        p.SupportsPendingObligationQueue = supportsPendingObligationQueue;
        p.SupportsDebtList               = supportsDebtList;
        p.DefaultOverdraftLimit          = defaultOverdraftLimit;
        _store.SaveProduct(p);
        return Reload(code);  // return to view mode after save
    }

    // ── Product balances ────────────────────────────────────────────────────
    public IActionResult OnPostAddProductBalance(int code, string fiCode, int balanceTypePrefix,
        string balanceName, string currencyCode, long balanceAddress, string glCode)
    {
        var p = Require(code);

        // Find or create a matching BalanceDefinition
        var def = _store.AllBalanceDefinitions
            .FirstOrDefault(d => d.FinancialIndicatorCode == fiCode && d.BalanceTypePrefix == balanceTypePrefix);
        if (def == null)
        {
            def = new BalanceDefinition
            {
                FinancialIndicatorCode = fiCode,
                BalanceTypePrefix      = balanceTypePrefix,
                Name                   = (balanceName ?? "").Trim(),
            };
            _store.SaveBalanceDefinition(def);
        }

        p.ProductBalances.Add(new ProductBalance
        {
            BalanceDefinitionId = def.Id,
            BalanceCode         = def.Code,
            BalanceName         = string.IsNullOrWhiteSpace(balanceName) ? def.Name : balanceName.Trim(),
            CurrencyCode        = (currencyCode ?? "USD").ToUpper().Trim(),
            BalanceAddress      = balanceAddress,
            GlCode              = (glCode ?? "").Trim(),
        });
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    public IActionResult OnPostUpdateProductBalance(int code, string balanceId, string glCode)
    {
        var p = Require(code);
        var b = p.ProductBalances.FirstOrDefault(x => x.Id == balanceId);
        if (b != null) b.GlCode = (glCode ?? "").Trim();
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    public IActionResult OnPostDeleteProductBalance(int code, string balanceId)
    {
        var p = Require(code);
        var b = p.ProductBalances.FirstOrDefault(x => x.Id == balanceId);
        if (b != null) p.ProductBalances.Remove(b);
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    // ── Debt balance addresses ───────────────────────────────────────────────
    public IActionResult OnPostAddDebtAddress(int code, long address, string currency)
    {
        var p = Require(code);
        if (address > 0 && !p.DebtBalanceAddresses.Any(d =>
                d.Address == address &&
                d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)))
            p.DebtBalanceAddresses.Add(new DebtBalanceRef
            {
                Address  = address,
                Currency = currency.ToUpper().Trim()
            });
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    public IActionResult OnPostUpdateDebtAddress(int code, long oldAddress, string currency, long newAddress)
    {
        var p   = Require(code);
        var row = p.DebtBalanceAddresses.FirstOrDefault(d =>
            d.Address == oldAddress &&
            d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));
        if (row != null) row.Address = newAddress;
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    public IActionResult OnPostRemoveDebtAddress(int code, long address, string currency)
    {
        var p   = Require(code);
        var row = p.DebtBalanceAddresses.FirstOrDefault(d =>
            d.Address == address &&
            d.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));
        if (row != null) p.DebtBalanceAddresses.Remove(row);
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    // ── Rate entries ─────────────────────────────────────────────────────────
    public IActionResult OnPostAddRate(int code, int rateCode, string rateName, string fiCodes)
    {
        var p = Require(code);
        if (rateCode > 0 && !p.Rates.Any(r => r.Code == rateCode))
            p.Rates.Add(new RateEntry
            {
                Code = rateCode,
                Name = rateName ?? "",
                FinancialIndicatorCodes = (fiCodes ?? "001")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            });
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    public IActionResult OnPostRemoveRate(int code, int rateCode)
    {
        var p   = Require(code);
        var row = p.Rates.FirstOrDefault(r => r.Code == rateCode);
        if (row != null) p.Rates.Remove(row);
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    // ── Dictionary of interest ───────────────────────────────────────────────
    public IActionResult OnPostAddDictEntry(int code, decimal threshold, decimal modifier)
    {
        var p = Require(code);
        p.DictionaryOfInterest[threshold] = modifier;
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    public IActionResult OnPostRemoveDictEntry(int code, decimal threshold)
    {
        var p = Require(code);
        p.DictionaryOfInterest.Remove(threshold);
        _store.SaveProduct(p);
        return Reload(code, edit: true);
    }

    // ── Delete product ───────────────────────────────────────────────────────
    public IActionResult OnPostDelete(int code)
    {
        _store.DeleteProduct(code);
        return RedirectToPage("/Products/Index");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private Product Require(int code) =>
        _store.GetProduct(code) ?? throw new InvalidOperationException($"Product {code} not found");

    private IActionResult Reload(int code, bool edit = false) =>
        edit ? RedirectToPage(new { code, edit = true })
             : RedirectToPage(new { code });

    private void LoadLinkedRates()
    {
        if (Product == null) return;
        LinkedRates = Product.Rates
            .Select(r => _store.GetInterestRate(r.Code))
            .Where(r => r != null)
            .Cast<InterestRate>()
            .ToList();
    }
}
