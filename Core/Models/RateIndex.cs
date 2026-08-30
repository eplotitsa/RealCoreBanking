namespace RealCoreBanking.Core.Models;

public enum RateIndexType   { ExternalApi, File, Web, Manual, Internal }
public enum RateIndexStatus { Active, Suspended, Archived }

public class RateIndex
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public RateIndexType Type { get; set; } = RateIndexType.Manual;
    public string SourceDescription { get; set; } = "";
    public string ConnectionParamsJson { get; set; } = "{}";
    public string Timezone { get; set; } = "UTC";
    public TimeOnly PublicationTime { get; set; }
    public int PollingFrequencyMin { get; set; } = 60;
    public RateIndexStatus Status { get; set; } = RateIndexStatus.Active;

    public decimal DeviationWarnPercent { get; set; }
    public decimal DeviationBlockPercent { get; set; }
    public decimal DeviationWarnBp { get; set; }
    public decimal DeviationBlockBp { get; set; }
    public int MaxAgePeriods { get; set; } = 1;

    public string Currency { get; set; } = "USD";
    public string Tenor { get; set; } = "NA"; // ON|1W|1M|3M|6M|1Y|NA
    public int Precision { get; set; } = 6;

    public List<RateValue> Values { get; set; } = new();

    public RateValue? GetCurrentRate() =>
        Values
            .Where(v => v.Status == RateValueStatus.Active)
            .OrderByDescending(v => v.ValueDate)
            .FirstOrDefault();

    public RateValue? GetRateAtDate(DateTime dt) =>
        Values
            .Where(v => v.Status == RateValueStatus.Active && v.ValueDate <= dt)
            .OrderByDescending(v => v.ValueDate)
            .FirstOrDefault();

    public void RegisterRate(RateValue value)
    {
        var existing = Values.FirstOrDefault(v =>
            v.ValueDate == value.ValueDate && v.Status == RateValueStatus.Active);
        if (existing != null)
            existing.Status = RateValueStatus.Superseded;

        Values.Add(value);
    }

    public bool CancelRate(DateTime valueDate)
    {
        var rate = Values.FirstOrDefault(v =>
            v.ValueDate == valueDate && v.Status == RateValueStatus.Active);
        if (rate == null) return false;
        rate.Status = RateValueStatus.Voided;
        return true;
    }
}
