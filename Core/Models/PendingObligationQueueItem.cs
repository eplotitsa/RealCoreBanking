namespace RealCoreBanking.Core.Models;

public class PendingObligationQueueItem
{
    public string BlockId { get; set; } = Guid.NewGuid().ToString();
    public string AccountId { get; set; } = "";
    public int BlockTypeId { get; set; }
    public string UniqueCode { get; set; } = "";

    // EXTERNAL_SYSTEM | CLIENT
    public string Initiator { get; set; } = "EXTERNAL_SYSTEM";

    // MOBILE_APP | INTERNET_BANK | BRANCH | CALL_CENTER | null
    public string? Channel { get; set; }

    public string? TargetRef { get; set; }
    public string? Purpose { get; set; }
    public string ReasonText { get; set; } = "";

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PHP";
    public short Priority { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    // Created|Active|WaitingFunds|PartialHold|Written|Cancelled|Expired|Bypassed
    public string Status { get; set; } = "Created";
}
