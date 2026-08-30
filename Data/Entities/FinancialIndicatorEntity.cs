using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

public class FinancialIndicatorEntity
{
    [Key] public string Code { get; set; } = "";
    public string Description { get; set; } = "";
}
