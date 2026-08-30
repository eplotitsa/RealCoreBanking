namespace RealCoreBanking.Core.Models;

public class HoldItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
