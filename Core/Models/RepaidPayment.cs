namespace RealCoreBanking.Core.Models;

public class RepaidPayment
{
    // Reference to the DebtComponent (multiple rows per component for partial repayments)
    public string DebtId { get; set; } = "";

    // Payment that performed the distribution
    public string PaymentId { get; set; } = "";

    public DateTime RepaymentDate { get; set; } = DateTime.Today;

    // Amount distributed (may be less than the full component amount)
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";
}
