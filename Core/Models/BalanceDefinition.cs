using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

/// <summary>
/// Catalog entry: defines a balance TYPE (logic).
/// Currency and GL address are assigned when the balance is added to a Product.
/// </summary>
public class BalanceDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];

    // Auto-computed: prefix(D2) + fiCode — not user-editable
    public string Code => BalanceTypePrefix.ToString("D2") + FinancialIndicatorCode;

    public string Name { get; set; } = "";

    // 1=IncreasesTotal  2=DecreasesTotal  3=NoEffect  4=BlocksTotal  5=BlocksOTB
    public int BalanceTypePrefix { get; set; } = 1;

    public string FinancialIndicatorCode { get; set; } = "001";

    public BalanceImpact Impact => BalanceTypePrefix switch
    {
        1 => BalanceImpact.IncreasesTotal,
        2 => BalanceImpact.DecreasesTotal,
        4 => BalanceImpact.BlocksTotal,
        _ => BalanceImpact.NoEffect,
    };

    public string BalanceTypePrefixLabel() =>
        BalanceTypeDefinition.FromPrefix(BalanceTypePrefix)?.Description
        ?? BalanceTypePrefix.ToString("D2");
}
