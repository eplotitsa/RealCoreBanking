using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InterestRates;

public class IndexDetailsModel : PageModel
{
    private readonly AccountStore _store;

    public RateIndex? Index { get; set; }

    public IndexDetailsModel(AccountStore store) => _store = store;

    public IActionResult OnGet(string code)
    {
        Index = _store.GetRateIndex(code);
        if (Index == null) return NotFound();
        return Page();
    }

    public IActionResult OnPostUpdate(string code, string name, RateIndexType type,
        string currency, string tenor, RateIndexStatus status,
        string sourceDescription, string connectionParamsJson, string timezone,
        string publicationTimeStr, int pollingFrequencyMin,
        decimal deviationWarnPercent, decimal deviationBlockPercent,
        decimal deviationWarnBp, decimal deviationBlockBp,
        int maxAgePeriods, int precision)
    {
        var idx = Require(code);
        idx.Name                  = name.Trim();
        idx.Type                  = type;
        idx.Currency              = currency.ToUpper().Trim();
        idx.Tenor                 = tenor.Trim();
        idx.Status                = status;
        idx.SourceDescription     = sourceDescription?.Trim() ?? "";
        idx.ConnectionParamsJson  = string.IsNullOrWhiteSpace(connectionParamsJson) ? "{}" : connectionParamsJson;
        idx.Timezone              = timezone.Trim();
        if (TimeOnly.TryParse(publicationTimeStr, out var pt)) idx.PublicationTime = pt;
        idx.PollingFrequencyMin   = pollingFrequencyMin;
        idx.DeviationWarnPercent  = deviationWarnPercent;
        idx.DeviationBlockPercent = deviationBlockPercent;
        idx.DeviationWarnBp       = deviationWarnBp;
        idx.DeviationBlockBp      = deviationBlockBp;
        idx.MaxAgePeriods         = maxAgePeriods;
        idx.Precision             = precision;
        _store.PersistRateIndex(code);  // save metadata changes to DB
        return Reload(code);
    }

    public IActionResult OnPostAddValue(string code, DateTime valueDate, decimal value,
        string author, RateInputMethod inputMethod)
    {
        Require(code);
        _store.AddIndexValue(code, new RateValue
        {
            ValueDate   = valueDate,
            Value       = value,
            Status      = RateValueStatus.Active,
            InputMethod = inputMethod,
            Author      = author?.Trim() ?? "",
            PublishedAt = DateTime.UtcNow,
            ReceivedAt  = DateTime.UtcNow,
        });
        return Reload(code);
    }

    public IActionResult OnPostVoidValue(string code, DateTime valueDate)
    {
        Require(code);
        _store.VoidIndexValue(code, valueDate);
        return Reload(code);
    }

    public IActionResult OnPostDelete(string code)
    {
        _store.DeleteRateIndex(code);
        return RedirectToPage("/InterestRates/Index");
    }

    private RateIndex Require(string code) =>
        _store.GetRateIndex(code) ?? throw new InvalidOperationException($"Index {code} not found");

    private RedirectToPageResult Reload(string code) =>
        RedirectToPage(new { code });
}
