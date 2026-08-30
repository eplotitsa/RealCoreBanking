using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

public enum InterestRatePurpose { Loan, Deposit, Both, Ftp }
public enum InterestRateType    { Fixed, Floating, Stepped }
public enum InterestUpdatePeriod{ Daily, Weekly, Monthly, Quarterly, Annually, OnIndexChange, NA }

public class InterestRate
{
    public int Code { get; set; }
    public string Name { get; set; } = "";
    public decimal CurrentRate { get; set; }
    public decimal PreviousRate { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    public DateTime? EffectiveTo { get; set; }

    public InterestRatePurpose Purpose { get; set; } = InterestRatePurpose.Deposit;
    public InterestRateType Type { get; set; } = InterestRateType.Fixed;
    public string Currency { get; set; } = "USD";

    public string RateIndexCode { get; set; } = "";
    public InterestUpdatePeriod UpdatePeriod { get; set; } = InterestUpdatePeriod.NA;

    public int FixingLagDays { get; set; }
    public int NotificationLagDays { get; set; }

    // Applied as: raw = indexBase × Multiplier + Spread/100
    public decimal Spread { get; set; }
    public decimal Multiplier { get; set; } = 1m;

    // Rounding
    public decimal RoundingStep { get; set; } = 0.05m;
    public RoundingRule RoundingRule { get; set; } = RoundingRule.HalfUp;

    // Limits
    public decimal Floor { get; set; }
    public decimal Cap { get; set; }
    public decimal RegulatoryMax { get; set; }

    public void UpdateRate(decimal indexBase)
    {
        PreviousRate = CurrentRate;
        var raw = indexBase * Multiplier + Spread / 100m;
        raw = ApplyRounding(raw);
        // 0 means "not set" — only apply limit when explicitly configured
        if (Cap   > 0) raw = Math.Min(raw, Cap);
        if (Floor > 0) raw = Math.Max(raw, Floor);
        if (RegulatoryMax > 0) raw = Math.Min(raw, RegulatoryMax);
        CurrentRate = raw;
        EffectiveFrom = DateTime.Today;
    }

    private decimal ApplyRounding(decimal value)
    {
        if (RoundingStep <= 0) return value;
        var steps = value / RoundingStep;
        steps = RoundingRule switch
        {
            RoundingRule.HalfUp      => Math.Round(steps, MidpointRounding.AwayFromZero),
            RoundingRule.Up          => Math.Ceiling(steps),
            RoundingRule.Down        => Math.Floor(steps),
            RoundingRule.AwayFromZero=> Math.Round(steps, MidpointRounding.AwayFromZero),
            _                        => Math.Round(steps, MidpointRounding.AwayFromZero),
        };
        return steps * RoundingStep;
    }

    public decimal CalculateInterest(decimal amount, DateTime from, DateTime to, string accrualBasis = "ACT/ACT")
    {
        var days = (int)(to - from).TotalDays;
        var daysInYear = accrualBasis switch
        {
            "ACT/360"  => 360m,
            "ACT/365F" => 365m,
            "30/360"   => 360m,
            "ACT/ACT"  => DateTime.IsLeapYear(from.Year) ? 366m : 365m,
            _          => 365m,
        };
        return Math.Round(amount * (CurrentRate / 100m) * days / daysInYear, 10);
    }
}
