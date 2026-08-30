namespace RealCoreBanking.Core.Models;

public class Enterprise
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "RealCoreBanking";
    public DateTime BankingDate { get; set; } = DateTime.Today;
    public string Timezone { get; set; } = "UTC+8";

    public void AdvanceDate(int days = 1) => BankingDate = BankingDate.AddDays(days);
    public void SetDate(DateTime date)    => BankingDate = date.Date;
}
