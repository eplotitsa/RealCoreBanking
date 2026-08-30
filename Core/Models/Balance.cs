using System.Text.Json.Serialization;
using RealCoreBanking.Core.Enums;

namespace RealCoreBanking.Core.Models;

public class Balance
{
    public long Address { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public int DecimalPlaces { get; set; } = 2;
    public RoundingRule RoundingRule { get; set; } = RoundingRule.AwayFromZero;
    public string FinancialIndicatorCode { get; set; } = "001";
    public string GlCode { get; set; } = "";

    [JsonIgnore]
    public int Prefix => (int)(Address / 100_000_000L);

    [JsonIgnore]
    public string Code => Prefix.ToString("D2") + FinancialIndicatorCode;

    [JsonIgnore]
    public BalanceImpact Impact => Prefix switch
    {
        1 => BalanceImpact.IncreasesTotal,
        2 => BalanceImpact.DecreasesTotal,
        4 => BalanceImpact.BlocksTotal,
        _ => BalanceImpact.NoEffect,
    };

    public void Add(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount must be non-negative");
        Amount = Round(Amount + amount);
    }

    public bool Subtract(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount must be non-negative");
        if (Amount < amount) return false;
        Amount = Round(Amount - amount);
        return true;
    }

    public decimal GetBalance() => Amount;

    private decimal Round(decimal value)
    {
        double scale = Math.Pow(10, DecimalPlaces);
        return RoundingRule switch
        {
            RoundingRule.Up     => Math.Ceiling(value * (decimal)scale) / (decimal)scale,
            RoundingRule.Down   => Math.Floor(value * (decimal)scale) / (decimal)scale,
            RoundingRule.ToEven => Math.Round(value, DecimalPlaces, MidpointRounding.ToEven),
            _                   => Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero),
        };
    }

    public override string ToString() =>
        $"[{Address:D10}] {Name} {Amount.ToString("N" + DecimalPlaces)} {Currency}";
}
