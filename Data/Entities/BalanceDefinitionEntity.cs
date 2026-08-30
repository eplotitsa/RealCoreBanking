using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

public class BalanceDefinitionEntity
{
    [Key] public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int BalanceTypePrefix { get; set; } = 1;
    public string FinancialIndicatorCode { get; set; } = "001";
}
