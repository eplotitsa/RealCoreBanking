namespace RealCoreBanking.Core.Models;

public class StandingOrder
{
    public string SoId { get; set; } = Guid.NewGuid().ToString();
    public string SoReference { get; set; } = "";
    public string Channel { get; set; } = "BRANCH"; // MOBILE_APP|INTERNET_BANK|BRANCH|CALL_CENTER

    public string SourceAccountId { get; set; } = "";

    // Beneficiary
    public string BeneficiaryType { get; set; } = "internal"; // internal|sepa_credit_transfer|swift|direct_debit|faster_payment
    public string? BeneficiaryAccountId { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? BeneficiaryIban { get; set; }
    public string? BeneficiaryBic { get; set; }
    public string? BeneficiaryBankName { get; set; }
    public string? BeneficiarySortCode { get; set; }

    // Amount
    public string AmountType { get; set; } = "fixed"; // fixed|full_balance|percentage|max_available
    public decimal? FixedAmount { get; set; }
    public decimal? Percentage { get; set; }
    public string Currency { get; set; } = "PHP";
    public string ReferenceText { get; set; } = "";

    // Schedule
    public string Frequency { get; set; } = "monthly"; // daily|weekly|bi_weekly|monthly|bi_monthly|quarterly|semi_annually|annually
    public int ExecutionDay { get; set; } = 1; // 1-28, or 31 = last day of month
    public string MonthEndRule { get; set; } = "last_banking_day"; // last_banking_day|last_calendar_day
    public string HolidayRule { get; set; } = "next_banking_day"; // next_banking_day|previous_banking_day|skip
    public DateTime FirstExecutionDate { get; set; } = DateTime.Today;

    // Termination
    public string TerminationType { get; set; } = "perpetual"; // perpetual|by_date|by_count
    public DateTime? TerminationDate { get; set; }
    public int? TerminationCount { get; set; }
    public int ExecutionsCompleted { get; set; }

    // Insufficient funds
    public string InsufficientFundsAction { get; set; } = "skip"; // skip|partial|wait

    // Lifecycle
    public string Status { get; set; } = "active"; // active|suspended|pending_first_execution|completed|cancelled
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    public DateTime? NextExecutionDate { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public string? LastExecutionStatus { get; set; } // success|failed|skipped|partial|null
}
