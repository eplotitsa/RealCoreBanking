namespace RealCoreBanking.Core.Models;

public class TestSession
{
    public string? AccountId { get; set; }
    public DateTime? NewBankingDay { get; set; }
    public List<TestTransaction> Transactions { get; set; } = new();
}
