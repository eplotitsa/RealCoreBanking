using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

public class GlAccountEntity
{
    [Key]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = "";

    [MaxLength(127)]
    public string Name { get; set; } = "";

    // Assets | Liability | OffBalance
    [MaxLength(20)]
    public string BalanceType { get; set; } = "Assets";
}
