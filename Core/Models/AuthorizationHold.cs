namespace RealCoreBanking.Core.Models;

public class AuthorizationHold
{
    public string HoldId { get; set; } = Guid.NewGuid().ToString();
    public string AccountId { get; set; } = "";
    public string CardId { get; set; } = "";
    public string AuthCode { get; set; } = "";
    public string Rrn { get; set; } = "";   // Retrieval Reference Number
    public string Stan { get; set; } = "";  // System Trace Audit Number
    public string NetworkTransactionId { get; set; } = "";

    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = "PHP";

    public decimal? SettlementAmount { get; set; }
    public string? SettlementCurrency { get; set; }

    public decimal? BillingAmount { get; set; }
    public string? BillingCurrency { get; set; }

    public string Mcc { get; set; } = "";       // Merchant Category Code
    public string? MerchantId { get; set; }
    public string? MerchantName { get; set; }

    // purchase|cash_advance|refund|pre_auth|completion
    public string AuthType { get; set; } = "purchase";

    // pending|captured|settled|reversed|expired|declined
    public string LifecycleStage { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime? CapturedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}
