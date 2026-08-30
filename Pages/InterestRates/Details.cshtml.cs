using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InterestRates;

public class DetailsModel : PageModel
{
    private readonly AccountStore _store;

    public InterestRate? Rate { get; set; }
    public RateIndex? LinkedIndex { get; set; }
    public IReadOnlyCollection<RateIndex> Indexes => _store.AllIndexes;

    public DetailsModel(AccountStore store) => _store = store;

    public IActionResult OnGet(int code)
    {
        Rate = _store.GetInterestRate(code);
        if (Rate == null) return NotFound();
        LoadLinkedIndex();
        return Page();
    }

    public IActionResult OnPostUpdate(int code, string name, string currency,
        InterestRatePurpose purpose, InterestRateType type, decimal currentRate,
        DateTime effectiveFrom, DateTime? effectiveTo,
        string rateIndexCode, InterestUpdatePeriod updatePeriod,
        int fixingLagDays, int notificationLagDays,
        decimal spread, decimal multiplier,
        decimal roundingStep, RoundingRule roundingRule,
        decimal floor, decimal cap, decimal regulatoryMax)
    {
        var r = Require(code);
        r.Name                = name.Trim();
        r.Currency            = currency.ToUpper().Trim();
        r.Purpose             = purpose;
        r.Type                = type;
        r.EffectiveFrom       = effectiveFrom;
        r.EffectiveTo         = effectiveTo;
        r.RateIndexCode       = rateIndexCode?.Trim() ?? "";
        r.UpdatePeriod        = updatePeriod;
        r.FixingLagDays       = fixingLagDays;
        r.NotificationLagDays = notificationLagDays;
        r.Spread              = spread;
        r.Multiplier          = multiplier == 0 ? 1m : multiplier;
        r.RoundingStep        = roundingStep;
        r.RoundingRule        = roundingRule;
        r.Floor               = floor;
        r.Cap                 = cap;
        r.RegulatoryMax       = regulatoryMax;

        // if linked to an index, derive CurrentRate from it
        if (!string.IsNullOrWhiteSpace(r.RateIndexCode))
        {
            var idx = _store.GetRateIndex(r.RateIndexCode);
            var cur = idx?.GetCurrentRate();
            if (cur != null)
                r.UpdateRate(cur.Value);
        }
        else
        {
            // manual fixed rate: accept the submitted value
            r.PreviousRate = r.CurrentRate;
            r.CurrentRate  = currentRate;
        }

        _store.PersistInterestRate(code);  // save changes to DB
        return Reload(code);
    }

    public IActionResult OnPostRefreshFromIndex(int code)
    {
        var r = Require(code);
        if (!string.IsNullOrWhiteSpace(r.RateIndexCode))
        {
            var idx = _store.GetRateIndex(r.RateIndexCode);
            var cur = idx?.GetCurrentRate();
            if (cur != null)
            {
                r.UpdateRate(cur.Value);
                _store.PersistInterestRate(code);
            }
        }
        return Reload(code);
    }

    public IActionResult OnPostDelete(int code)
    {
        _store.DeleteInterestRate(code);
        return RedirectToPage("/InterestRates/Index");
    }

    private InterestRate Require(int code) =>
        _store.GetInterestRate(code) ?? throw new InvalidOperationException($"Rate {code} not found");

    private RedirectToPageResult Reload(int code) =>
        RedirectToPage(new { code });

    private void LoadLinkedIndex()
    {
        if (Rate == null || string.IsNullOrWhiteSpace(Rate.RateIndexCode)) return;
        LinkedIndex = _store.GetRateIndex(Rate.RateIndexCode);
    }
}
