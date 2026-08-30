using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

/// <summary>
/// Stores the full Product as JSON so nested lists (ProductBalances, GlCodes, etc.)
/// require no extra tables. Trade-off: no relational queries on fields inside JSON.
/// </summary>
public class ProductEntity
{
    [Key] public int Code { get; set; }
    public string Data { get; set; } = "{}";
}
