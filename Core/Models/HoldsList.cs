namespace RealCoreBanking.Core.Models;

public class HoldsList
{
    public List<HoldItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Sum(h => h.Amount);

    public void Add(HoldItem hold) => Items.Add(hold);

    public bool Remove(string holdId)
    {
        var hold = Items.FirstOrDefault(h => h.Id == holdId);
        if (hold == null) return false;
        Items.Remove(hold);
        return true;
    }
}
