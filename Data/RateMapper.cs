using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data;

/// <summary>Bidirectional mapping between domain models and EF entities.</summary>
public static class RateMapper
{
    // ─── InterestRate ────────────────────────────────────────────────────────

    public static InterestRate ToDomain(InterestRateEntity e) => new()
    {
        Code                = e.Code,
        Name                = e.Name,
        CurrentRate         = e.CurrentRate,
        PreviousRate        = e.PreviousRate,
        EffectiveFrom       = e.EffectiveFrom,
        EffectiveTo         = e.EffectiveTo,
        Purpose             = Enum.Parse<InterestRatePurpose>(e.Purpose),
        Type                = Enum.Parse<InterestRateType>(e.Type),
        Currency            = e.Currency,
        RateIndexCode       = e.RateIndexCode,
        UpdatePeriod        = Enum.Parse<InterestUpdatePeriod>(e.UpdatePeriod),
        FixingLagDays       = e.FixingLagDays,
        NotificationLagDays = e.NotificationLagDays,
        Spread              = e.Spread,
        Multiplier          = e.Multiplier,
        RoundingStep        = e.RoundingStep,
        RoundingRule        = Enum.Parse<RoundingRule>(e.RoundingRule),
        Floor               = e.Floor,
        Cap                 = e.Cap,
        RegulatoryMax       = e.RegulatoryMax,
    };

    public static InterestRateEntity ToEntity(InterestRate r) => new()
    {
        Code                = r.Code,
        Name                = r.Name,
        CurrentRate         = r.CurrentRate,
        PreviousRate        = r.PreviousRate,
        EffectiveFrom       = r.EffectiveFrom,
        EffectiveTo         = r.EffectiveTo,
        Purpose             = r.Purpose.ToString(),
        Type                = r.Type.ToString(),
        Currency            = r.Currency,
        RateIndexCode       = r.RateIndexCode,
        UpdatePeriod        = r.UpdatePeriod.ToString(),
        FixingLagDays       = r.FixingLagDays,
        NotificationLagDays = r.NotificationLagDays,
        Spread              = r.Spread,
        Multiplier          = r.Multiplier,
        RoundingStep        = r.RoundingStep,
        RoundingRule        = r.RoundingRule.ToString(),
        Floor               = r.Floor,
        Cap                 = r.Cap,
        RegulatoryMax       = r.RegulatoryMax,
    };

    // ─── RateIndex ───────────────────────────────────────────────────────────

    public static RateIndex ToDomain(RateIndexEntity e) => new()
    {
        Code                  = e.Code,
        Name                  = e.Name,
        Type                  = Enum.Parse<RateIndexType>(e.Type),
        SourceDescription     = e.SourceDescription,
        ConnectionParamsJson  = e.ConnectionParamsJson,
        Timezone              = e.Timezone,
        PublicationTime       = TimeOnly.TryParse(e.PublicationTime, out var t) ? t : new TimeOnly(15, 0),
        PollingFrequencyMin   = e.PollingFrequencyMin,
        Status                = Enum.Parse<RateIndexStatus>(e.Status),
        DeviationWarnPercent  = e.DeviationWarnPercent,
        DeviationBlockPercent = e.DeviationBlockPercent,
        DeviationWarnBp       = e.DeviationWarnBp,
        DeviationBlockBp      = e.DeviationBlockBp,
        MaxAgePeriods         = e.MaxAgePeriods,
        Currency              = e.Currency,
        Tenor                 = e.Tenor,
        Precision             = e.Precision,
        Values                = e.Values.Select(ToDomain).ToList(),
    };

    // metadata only — values are managed separately
    public static RateIndexEntity ToEntity(RateIndex idx) => new()
    {
        Code                  = idx.Code,
        Name                  = idx.Name,
        Type                  = idx.Type.ToString(),
        SourceDescription     = idx.SourceDescription,
        ConnectionParamsJson  = idx.ConnectionParamsJson,
        Timezone              = idx.Timezone,
        PublicationTime       = idx.PublicationTime.ToString("HH:mm"),
        PollingFrequencyMin   = idx.PollingFrequencyMin,
        Status                = idx.Status.ToString(),
        DeviationWarnPercent  = idx.DeviationWarnPercent,
        DeviationBlockPercent = idx.DeviationBlockPercent,
        DeviationWarnBp       = idx.DeviationWarnBp,
        DeviationBlockBp      = idx.DeviationBlockBp,
        MaxAgePeriods         = idx.MaxAgePeriods,
        Currency              = idx.Currency,
        Tenor                 = idx.Tenor,
        Precision             = idx.Precision,
    };

    // ─── RateValue ───────────────────────────────────────────────────────────

    public static RateValue ToDomain(RateValueEntity e) => new()
    {
        ValueDate   = e.ValueDate,
        Value       = e.Value,
        Status      = Enum.Parse<RateValueStatus>(e.Status),
        PublishedAt = e.PublishedAt,
        ReceivedAt  = e.ReceivedAt,
        InputMethod = Enum.Parse<RateInputMethod>(e.InputMethod),
        Author      = e.Author,
        Approver    = e.Approver,
    };

    public static RateValueEntity ToEntity(RateValue v, string indexCode) => new()
    {
        IndexCode   = indexCode,
        ValueDate   = v.ValueDate,
        Value       = v.Value,
        Status      = v.Status.ToString(),
        PublishedAt = v.PublishedAt,
        ReceivedAt  = v.ReceivedAt,
        InputMethod = v.InputMethod.ToString(),
        Author      = v.Author,
        Approver    = v.Approver,
    };
}
