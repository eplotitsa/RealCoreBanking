namespace RealCoreBanking.Core.Models;

public class RateEntry
{
    public int Code { get; set; }
    public string Name { get; set; } = "";
    public List<string> FinancialIndicatorCodes { get; set; } = new();
}
