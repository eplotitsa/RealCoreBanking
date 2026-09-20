namespace RealCoreBanking.Core.Models;

public class RateEntry
{
    public int Code { get; set; }
    public string Name { get; set; } = "";
    // Balance address whose amount this rate is calculated on
    public long AccrualBaseAddress { get; set; }
    // Balance address where accrued interest is credited
    public long AccrualTargetAddress { get; set; }
}
