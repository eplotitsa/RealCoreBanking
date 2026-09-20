using Microsoft.AspNetCore.Mvc.RazorPages;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Services;

namespace RealCoreBanking.Pages.InternalAccounts;

public class IndexModel : PageModel
{
    private readonly AccountStore _store;
    public IndexModel(AccountStore store) => _store = store;

    public IReadOnlyCollection<InternalAccount> Accounts => _store.AllInternalAccounts;

    public void OnGet() { }
}
