using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

/// <summary>
/// Stores the full CurrentAccount as a JSON blob — same pattern as ProductEntity.
/// Balances, HoldsList, DebtArray and all nested lists are serialized inside Data.
/// </summary>
public class CurrentAccountEntity
{
    [Key] public string AccountId { get; set; } = "";
    public string Data { get; set; } = "{}";
}
