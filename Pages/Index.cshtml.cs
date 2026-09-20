using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    public IReadOnlyCollection<CurrentAccount> Accounts => _store.All;
    public IReadOnlyList<string> Events => _store.RecentEvents;

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet() { }

    // Resolves the effective balance-type prefix the same way Details page does.
    private int EffectivePrefix(Balance b, Product? product)
    {
        if (b.BalanceTypePrefix > 0) return b.BalanceTypePrefix;
        if (product != null)
        {
            var pb = product.ProductBalances.FirstOrDefault(x => x.BalanceAddress == b.Address);
            if (pb != null && pb.BalanceCode.Length >= 2 &&
                int.TryParse(pb.BalanceCode[..2], out var pfx))
                return pfx;
        }
        return (int)(b.Address / 100_000_000L);
    }

    // OTB = Σ(pfx=1) − Σ(pfx=2) − Σ(pfx=4) − Σ(pfx=5) + Σ(pfx=6)
    public decimal GetOtb(CurrentAccount a)
    {
        var prod = _store.GetProduct(a.ProductCode);
        return a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 1).Sum(b => b.Amount)
             - a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 2).Sum(b => b.Amount)
             - a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 4).Sum(b => b.Amount)
             - a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 5).Sum(b => b.Amount)
             + a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 6).Sum(b => b.Amount);
    }

    // TotalBalance = Σ(pfx=1) − Σ(pfx=2)
    public decimal GetTotalBalance(CurrentAccount a)
    {
        var prod = _store.GetProduct(a.ProductCode);
        return a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 1).Sum(b => b.Amount)
             - a.Balances.Items.Where(b => EffectivePrefix(b, prod) == 2).Sum(b => b.Amount);
    }

    public IActionResult OnPostLoadDemo()
    {
        var phpBalances = new[]
        {
            new Balance { Address = 100_000_000L, Name = "Основная сумма PHP",              Currency = "PHP", FinancialIndicatorCode = "001", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
            new Balance { Address = 300_000_001L, Name = "Начисленные %% на основн. PHP",   Currency = "PHP", FinancialIndicatorCode = "002", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
            new Balance { Address = 200_000_002L, Name = "Блокировки по осн. счёту PHP",    Currency = "PHP", FinancialIndicatorCode = "005", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
            new Balance { Address = 210_000_003L, Name = "Использованный овердрафт PHP",    Currency = "PHP", FinancialIndicatorCode = "007", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
            new Balance { Address = 210_000_004L, Name = "Начисленные %% на овердрафт PHP", Currency = "PHP", FinancialIndicatorCode = "002", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
        };

        var usdBalances = new[]
        {
            new Balance { Address = 100_000_000L, Name = "Основная сумма USD",           Currency = "USD", FinancialIndicatorCode = "001", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
            new Balance { Address = 300_000_001L, Name = "Начисленные %% на основн. USD", Currency = "USD", FinancialIndicatorCode = "002", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
            new Balance { Address = 200_000_002L, Name = "Блокировки по осн. счёту USD", Currency = "USD", FinancialIndicatorCode = "005", DecimalPlaces = 3, RoundingRule = RoundingRule.AwayFromZero },
        };

        var product = new Product
        {
            Code          = 123,
            Name          = "Текущий счёт (демо)",
            AccrualBasis  = "ACT/ACT",
            BalanceType   = BalanceAccountType.Passive,
            DebtBalanceAddresses = new()
            {
                new DebtBalanceRef { Address = 210_000_003L, Currency = "PHP" },
                new DebtBalanceRef { Address = 210_000_004L, Currency = "PHP" },
            },
            Rates = new()
            {
                new RateEntry { Code = 2021, Name = "current account rate php" },
                new RateEntry { Code = 2022, Name = "current account rate usd" },
            },
            GlCodes = new()
            {
                new GlCodeEntry { Currency = "PHP", FinancialIndicatorCode = "001", GlCode = "00000001" },
                new GlCodeEntry { Currency = "PHP", FinancialIndicatorCode = "002", GlCode = "00000002" },
                new GlCodeEntry { Currency = "PHP", FinancialIndicatorCode = "007", GlCode = "00000007" },
                new GlCodeEntry { Currency = "USD", FinancialIndicatorCode = "001", GlCode = "10000001" },
                new GlCodeEntry { Currency = "USD", FinancialIndicatorCode = "002", GlCode = "10000002" },
            },
        };

        var phpRate = new InterestRate
        {
            Code            = 2021,
            Name            = "current account rate php",
            CurrentRate     = 15m,
            Purpose         = InterestRatePurpose.Deposit,
            Type            = InterestRateType.Fixed,
            Currency        = "PHP",
            RateIndexCode   = "INTERNAL_FTP",
            UpdatePeriod    = InterestUpdatePeriod.NA,
            Spread          = 0,
            Multiplier      = 1m,
            RoundingStep    = 0.05m,
            RoundingRule    = RoundingRule.HalfUp,
            Floor           = 0,
            Cap             = 10m,
            RegulatoryMax   = 10m,
            EffectiveFrom   = new DateTime(2026, 2, 21),
        };

        var usdRate = new InterestRate
        {
            Code            = 2022,
            Name            = "current account rate usd",
            CurrentRate     = 15m,
            Purpose         = InterestRatePurpose.Deposit,
            Type            = InterestRateType.Fixed,
            Currency        = "USD",
            RateIndexCode   = "INTERNAL_FTP",
            UpdatePeriod    = InterestUpdatePeriod.NA,
            Spread          = 0,
            Multiplier      = 1m,
            RoundingStep    = 0.05m,
            RoundingRule    = RoundingRule.HalfUp,
            Floor           = 0,
            Cap             = 10m,
            RegulatoryMax   = 10m,
            EffectiveFrom   = new DateTime(2026, 2, 21),
        };

        var rateIndex = new RateIndex
        {
            Code              = "INTERNAL_FTP",
            Name              = "Внутренняя ставка FTP",
            Type              = RateIndexType.Manual,
            Timezone          = "UTC+8",
            Status            = RateIndexStatus.Active,
            DeviationWarnPercent  = 10m,
            DeviationBlockPercent = 20m,
            DeviationWarnBp       = 10m,
            DeviationBlockBp      = 20m,
            Currency          = "PHP",
            Tenor             = "NA",
            Values = new()
            {
                new RateValue
                {
                    ValueDate   = new DateTime(2026, 2, 21, 15, 0, 0),
                    Value       = 15m,
                    Status      = RateValueStatus.Active,
                    InputMethod = RateInputMethod.Manual,
                    Author      = "Yevhen Plotytsya",
                    PublishedAt = new DateTime(2026, 2, 21, 15, 0, 0),
                    ReceivedAt  = new DateTime(2026, 2, 21, 15, 0, 0),
                },
            },
        };

        var account = CurrentAccount.Open(
            accountNumber:      "DEMO-PHP-USD-" + DateTime.Now.ToString("HHmmss"),
            bookType:           "main",
            balanceAccountType: BalanceAccountType.Passive,
            productCode:        123,
            customerIds:        new List<string> { "001-0001" },
            balances:           phpBalances.Concat(usdBalances));

        account.OverdraftLimit = 2000m;

        _store.Add(account);
        _store.AddProduct(product);
        _store.AddInterestRate(phpRate);
        _store.AddInterestRate(usdRate);
        _store.AddRateIndex(rateIndex);

        return RedirectToPage("/Account/Details", new { id = account.AccountId });
    }
}
