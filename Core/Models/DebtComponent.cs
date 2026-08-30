namespace RealCoreBanking.Core.Models;

public class DebtComponent
{
    public string FinancialIndicatorCode { get; set; } = "001";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string Description { get; set; } = "";
}
