namespace RealCoreBanking.Core.Models;

public class InternalDebitAuthority
{
    public string AuthorityId { get; set; } = Guid.NewGuid().ToString();
    public string AuthorityReference { get; set; } = "";

    // loan_agreement|credit_line_agreement|standalone_consent|account_terms
    public string BasisType { get; set; } = "standalone_consent";
    public string? BasisDocumentRef { get; set; }
    public string? BasisClause { get; set; }

    public string SourceAccountId { get; set; } = "";
    public string TargetAccountId { get; set; } = "";
    public string? AllocationRuleId { get; set; }

    // schedule_due_date|billing_cycle|event|on_demand
    public string TriggerType { get; set; } = "schedule_due_date";

    // amortization_schedule|billing_statement|fixed|full_outstanding
    public string AmountSource { get; set; } = "fixed";
    public decimal? FixedAmount { get; set; }
    public string Currency { get; set; } = "PHP";
    public string? ExecutionTimeRule { get; set; }

    // partial|skip|queue
    public string InsufficientFundsAction { get; set; } = "partial";
    public string? RetryPolicyId { get; set; }
    public int RetryWindowDays { get; set; }
    public decimal? MinPartialAmount { get; set; }

    public int Priority { get; set; }
    public bool HoldPlacement { get; set; }

    // active|suspended|completed|cancelled
    public string Status { get; set; } = "active";

    public DateTime EffectiveDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public string? ConsentEvidenceRef { get; set; }
    public string? DisclosureRef { get; set; }
}
