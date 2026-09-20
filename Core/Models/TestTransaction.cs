namespace RealCoreBanking.Core.Models;

public class TestTransaction
{
    public DateTime Date { get; set; } = DateTime.Today;
    public long BalanceAddress { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
}
