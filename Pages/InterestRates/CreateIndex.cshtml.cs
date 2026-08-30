using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InterestRates;

public class CreateIndexModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty] public string Code { get; set; } = "";
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public RateIndexType Type { get; set; } = RateIndexType.Manual;
    [BindProperty] public string Currency { get; set; } = "PHP";
    [BindProperty] public string Tenor { get; set; } = "NA";
    [BindProperty] public RateIndexStatus Status { get; set; } = RateIndexStatus.Active;
    [BindProperty] public string SourceDescription { get; set; } = "";
    [BindProperty] public string ConnectionParamsJson { get; set; } = "{}";
    [BindProperty] public string Timezone { get; set; } = "UTC+8";
    [BindProperty] public string PublicationTimeStr { get; set; } = "15:00";
    [BindProperty] public int PollingFrequencyMin { get; set; } = 60;
    [BindProperty] public decimal DeviationWarnPercent { get; set; }
    [BindProperty] public decimal DeviationBlockPercent { get; set; }
    [BindProperty] public decimal DeviationWarnBp { get; set; }
    [BindProperty] public decimal DeviationBlockBp { get; set; }
    [BindProperty] public int MaxAgePeriods { get; set; } = 1;
    [BindProperty] public int Precision { get; set; } = 6;

    public CreateIndexModel(AccountStore store) => _store = store;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Code)) ModelState.AddModelError("Code", "Введите код индекса");
        if (string.IsNullOrWhiteSpace(Name)) ModelState.AddModelError("Name", "Введите название");
        if (_store.GetRateIndex(Code.Trim()) != null)
            ModelState.AddModelError("Code", $"Индекс {Code} уже существует");
        if (!ModelState.IsValid) return Page();

        if (!TimeOnly.TryParse(PublicationTimeStr, out var pubTime))
            pubTime = new TimeOnly(15, 0);

        var index = new RateIndex
        {
            Code                  = Code.Trim().ToUpper(),
            Name                  = Name.Trim(),
            Type                  = Type,
            Currency              = Currency.ToUpper().Trim(),
            Tenor                 = Tenor.Trim(),
            Status                = Status,
            SourceDescription     = SourceDescription.Trim(),
            ConnectionParamsJson  = string.IsNullOrWhiteSpace(ConnectionParamsJson) ? "{}" : ConnectionParamsJson,
            Timezone              = Timezone.Trim(),
            PublicationTime       = pubTime,
            PollingFrequencyMin   = PollingFrequencyMin,
            DeviationWarnPercent  = DeviationWarnPercent,
            DeviationBlockPercent = DeviationBlockPercent,
            DeviationWarnBp       = DeviationWarnBp,
            DeviationBlockBp      = DeviationBlockBp,
            MaxAgePeriods         = MaxAgePeriods,
            Precision             = Precision,
        };

        _store.AddRateIndex(index);
        return RedirectToPage("/InterestRates/IndexDetails", new { code = index.Code });
    }
}
