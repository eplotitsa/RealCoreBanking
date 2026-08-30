namespace RealCoreBanking.Core.Models;

public class RepaidPayment
{
    public string FinancialIndicatorCode { get; set; } = "001";
    public DateTime EntryDate { get; set; }
    public DateTime RepaymentDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}
