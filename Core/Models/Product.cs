using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

public class Product
{
    public int Code { get; set; }
    public string Name { get; set; } = "";

    // ACT/360 | ACT/365F | 30/360 | ACT/ACT
    public string AccrualBasis { get; set; } = "ACT/ACT";

    public BalanceAccountType BalanceType { get; set; }

    // Balances assigned to this product (balance definition + currency + address + GL)
    public List<ProductBalance> ProductBalances { get; set; } = new();

    // Addresses of balances that represent debt on this product
    public List<DebtBalanceRef> DebtBalanceAddresses { get; set; } = new();

    // Rate codes linked to the product
    public List<RateEntry> Rates { get; set; } = new();

    // GL codes: currency → financial indicator code → GL account number
    public List<GlCodeEntry> GlCodes { get; set; } = new();

    // Tiered interest modifier: balance threshold → rate delta (%)
    // Sorted ascending by threshold. null = not used.
    public SortedDictionary<decimal, decimal> DictionaryOfInterest { get; set; } = new();

    // Feature support flags (null-style via bool: false = feature disabled)
    public bool SupportsStandingOrders { get; set; }
    public bool SupportsDirectDebit { get; set; }
    public bool SupportsAuthorizationHolds { get; set; }
    public bool SupportsPendingObligationQueue { get; set; }

    // Default overdraft limit for new accounts opened under this product
    public decimal DefaultOverdraftLimit { get; set; }

    public string? GetGlCode(string currency, string fiCode) =>
        GlCodes.FirstOrDefault(g =>
            g.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase) &&
            g.FinancialIndicatorCode == fiCode)?.GlCode;

    public int GetDaysInYear(DateTime date) => AccrualBasis switch
    {
        "ACT/360"  => 360,
        "ACT/365F" => 365,
        "30/360"   => 360,
        "ACT/ACT"  => DateTime.IsLeapYear(date.Year) ? 366 : 365,
        _          => 365,
    };
}
