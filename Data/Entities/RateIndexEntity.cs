using System.ComponentModel.DataAnnotations;

namespace RealCoreBanking.Data.Entities;

/// <summary>Row in RateIndexes table — stores metadata only, values live in RateValues.</summary>
public class RateIndexEntity
{
    [Key] public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Manual";           // enum stored as string
    public string SourceDescription { get; set; } = "";
    public string ConnectionParamsJson { get; set; } = "{}";
    public string Timezone { get; set; } = "UTC";
    public string PublicationTime { get; set; } = "15:00"; // "HH:mm"
    public int PollingFrequencyMin { get; set; } = 60;
    public string Status { get; set; } = "Active";         // enum stored as string
    public decimal DeviationWarnPercent { get; set; }
    public decimal DeviationBlockPercent { get; set; }
    public decimal DeviationWarnBp { get; set; }
    public decimal DeviationBlockBp { get; set; }
    public int MaxAgePeriods { get; set; } = 1;
    public string Currency { get; set; } = "USD";
    public string Tenor { get; set; } = "NA";
    public int Precision { get; set; } = 6;

    // navigation
    public List<RateValueEntity> Values { get; set; } = new();
}

/// <summary>One published value for a rate index.</summary>
public class RateValueEntity
{
    public int Id { get; set; }                      // auto PK
    public string IndexCode { get; set; } = "";
    public RateIndexEntity? Index { get; set; }      // FK navigation

    public DateTime ValueDate { get; set; }
    public decimal Value { get; set; }
    public string Status { get; set; } = "Active";   // enum as string
    public DateTime? PublishedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string InputMethod { get; set; } = "Manual";
    public string Author { get; set; } = "";
    public string Approver { get; set; } = "";
}
