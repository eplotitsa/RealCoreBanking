using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InterestRates;

public class CreateModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty] public int Code { get; set; }
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Currency { get; set; } = "PHP";
    [BindProperty] public InterestRatePurpose Purpose { get; set; } = InterestRatePurpose.Deposit;
    [BindProperty] public InterestRateType Type { get; set; } = InterestRateType.Fixed;
    [BindProperty] public decimal CurrentRate { get; set; }
    [BindProperty] public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    [BindProperty] public DateTime? EffectiveTo { get; set; }
    [BindProperty] public string RateIndexCode { get; set; } = "";
    [BindProperty] public InterestUpdatePeriod UpdatePeriod { get; set; } = InterestUpdatePeriod.NA;
    [BindProperty] public int FixingLagDays { get; set; }
    [BindProperty] public int NotificationLagDays { get; set; }
    [BindProperty] public decimal Spread { get; set; }
    [BindProperty] public decimal Multiplier { get; set; } = 1m;
    [BindProperty] public decimal RoundingStep { get; set; } = 0.05m;
    [BindProperty] public RoundingRule RoundingRule { get; set; } = RoundingRule.HalfUp;
    [BindProperty] public decimal Floor { get; set; }
    [BindProperty] public decimal Cap { get; set; }
    [BindProperty] public decimal RegulatoryMax { get; set; }

    public IReadOnlyCollection<RateIndex> Indexes => _store.AllIndexes;

    public CreateModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (Code <= 0) ModelState.AddModelError("Code", "Код должен быть > 0");
        if (string.IsNullOrWhiteSpace(Name)) ModelState.AddModelError("Name", "Введите название");
        if (_store.GetInterestRate(Code) != null)
            ModelState.AddModelError("Code", $"Ставка {Code} уже существует");
        if (!ModelState.IsValid) return Page();

        var rate = new InterestRate
        {
            Code                 = Code,
            Name                 = Name.Trim(),
            Currency             = Currency.ToUpper().Trim(),
            Purpose              = Purpose,
            Type                 = Type,
            CurrentRate          = CurrentRate,
            EffectiveFrom        = EffectiveFrom,
            EffectiveTo          = EffectiveTo,
            RateIndexCode        = RateIndexCode.Trim(),
            UpdatePeriod         = UpdatePeriod,
            FixingLagDays        = FixingLagDays,
            NotificationLagDays  = NotificationLagDays,
            Spread               = Spread,
            Multiplier           = Multiplier == 0 ? 1m : Multiplier,
            RoundingStep         = RoundingStep,
            RoundingRule         = RoundingRule,
            Floor                = Floor,
            Cap                  = Cap,
            RegulatoryMax        = RegulatoryMax,
        };

        // if linked to an index, derive CurrentRate immediately
        if (!string.IsNullOrWhiteSpace(rate.RateIndexCode))
        {
            var idx = _store.GetRateIndex(rate.RateIndexCode);
            var cur = idx?.GetCurrentRate();
            if (cur != null)
                rate.UpdateRate(cur.Value);
        }

        _store.AddInterestRate(rate);
        return RedirectToPage("/InterestRates/Details", new { code = rate.Code });
    }
}
