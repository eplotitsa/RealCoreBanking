namespace RealCoreBanking.Core.Models;

public enum RateValueStatus { PendingApproval, Active, Superseded, Voided }
public enum RateInputMethod  { Auto, Manual, Import }

public class RateValue
{
    public DateTime ValueDate { get; set; }
    public decimal Value { get; set; }
    public RateValueStatus Status { get; set; } = RateValueStatus.Active;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public RateInputMethod InputMethod { get; set; } = RateInputMethod.Manual;
    public string Author { get; set; } = "";
    public string Approver { get; set; } = "";
}
