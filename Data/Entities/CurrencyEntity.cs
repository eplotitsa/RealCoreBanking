using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

public class CurrencyEntity
{
    [Key]
    public string AlphaCode    { get; set; } = "";
    public string NumericCode  { get; set; } = "";
    public string Name         { get; set; } = "";
    public int    DecimalPlaces{ get; set; } = 2;
    public string RoundingRule { get; set; } = "AwayFromZero";
}
