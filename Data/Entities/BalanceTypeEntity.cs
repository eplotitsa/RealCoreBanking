using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

public class BalanceTypeEntity
{
    [Key] public string Code { get; set; } = "";
    public string Description { get; set; } = "";
}
