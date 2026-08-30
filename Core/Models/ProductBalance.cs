namespace RealCoreBanking.Core.Models;

/// <summary>
/// A balance definition assigned to a product with a specific currency and GL address.
/// This is the instance record: balance catalog entry + currency + physical address + GL code.
/// </summary>
public class ProductBalance
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];

    // Reference to the BalanceDefinition catalog entry
    public string BalanceDefinitionId { get; set; } = "";

    // Denormalized for display (resolved from catalog at load time)
    public string BalanceCode { get; set; } = "";  // e.g. "01001"
    public string BalanceName { get; set; } = "";

    // Currency assigned at product level
    public string CurrencyCode { get; set; } = "USD";

    // Physical 10-digit ledger address
    public long BalanceAddress { get; set; }

    // General Ledger account code
    public string GlCode { get; set; } = "";
}
