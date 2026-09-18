namespace RealCoreBanking.Core.Models;

public class DebtComponent
{
    public string DebtId { get; set; } = Guid.NewGuid().ToString("N")[..16];

    // Order of repayment distribution (lower = repaid first)
    public int Order { get; set; }

    // Balance address in the product that this debt component maps to
    public long Address { get; set; }

    public string Currency { get; set; } = "USD";

    // B_date: when the debt was calculated (billing date)
    public DateTime BillingDate { get; set; } = DateTime.Today;

    // D_date: when the debt became mandatory (due date)
    public DateTime DueDate { get; set; } =DateTime.Today.AddDays(5);

    public decimal Amount { get; set; }
}
