using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

/// <summary>Row in InterestRates table.</summary>
public class InterestRateEntity
{
    [Key] public int Code { get; set; }
    public string Name { get; set; } = "";
    public decimal CurrentRate { get; set; }
    public decimal PreviousRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Purpose { get; set; } = "Deposit";    // enum as string
    public string Type { get; set; } = "Fixed";         // enum as string
    public string Currency { get; set; } = "USD";
    public string RateIndexCode { get; set; } = "";
    public string UpdatePeriod { get; set; } = "NA";    // enum as string
    public int FixingLagDays { get; set; }
    public int NotificationLagDays { get; set; }
    public decimal Spread { get; set; }
    public decimal Multiplier { get; set; } = 1;
    public decimal RoundingStep { get; set; } = 0.05m;
    public string RoundingRule { get; set; } = "HalfUp"; // enum as string
    public decimal Floor { get; set; }
    public decimal Cap { get; set; }
    public decimal RegulatoryMax { get; set; }
}
