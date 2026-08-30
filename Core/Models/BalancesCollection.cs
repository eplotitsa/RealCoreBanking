using System.Text.Json.Serialization;

namespace RealCoreBanking.Core.Models;

public class BalancesCollection
{
    public List<Balance> Items { get; set; } = new();

    // Σ prefix=01 AND indicator=001 (main amount assets)
    [JsonIgnore]
    public decimal TotalAssets =>
        Items.Where(b => b.Prefix == 1 && b.FinancialIndicatorCode == "001").Sum(b => b.Amount);

    // Σ prefix=02 (decreasing balances)
    [JsonIgnore]
    public decimal TotalLiabilities =>
        Items.Where(b => b.Prefix == 2).Sum(b => b.Amount);

    // Σ prefix=04 (blocks)
    [JsonIgnore]
    public decimal TotalBlocked =>
        Items.Where(b => b.Prefix == 4).Sum(b => b.Amount);

    // Available = assets - liabilities - blocked
    [JsonIgnore]
    public decimal AvailableAmount => TotalAssets - TotalLiabilities - TotalBlocked;

    // Interest base = assets - liabilities (blocks not subtracted)
    [JsonIgnore]
    public decimal InterestBase => TotalAssets - TotalLiabilities;

    // Total balance = all prefix=01 minus all prefix=02
    [JsonIgnore]
    public decimal TotalBalance =>
        Items.Where(b => b.Prefix == 1).Sum(b => b.Amount)
        - Items.Where(b => b.Prefix == 2).Sum(b => b.Amount);

    public IEnumerable<string> Currencies =>
        Items.Select(b => b.Currency).Distinct(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Balance> GetByCurrency(string currency) =>
        Items.Where(b => b.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
             .ToList().AsReadOnly();

    public void Add(Balance balance) => Items.Add(balance);
    public bool Remove(Balance balance) => Items.Remove(balance);

    public Balance? GetByAddress(long address) =>
        Items.FirstOrDefault(b => b.Address == address);

    public Balance? GetByAddress(long address, string currency) =>
        Items.FirstOrDefault(b =>
            b.Address == address &&
            b.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));
}
