using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.Products;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<Product> Products { get; set; } = new();

    public IndexModel(AccountStore store) => _store = store;

    public void OnGet()
    {
        Products = _store.AllProducts
            .Where(p => string.IsNullOrEmpty(Search) ||
                p.Code.ToString().Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Code)
            .ToList();
    }

    public IActionResult OnPostDelete(int code)
    {
        _store.DeleteProduct(code);
        return RedirectToPage();
    }
}
