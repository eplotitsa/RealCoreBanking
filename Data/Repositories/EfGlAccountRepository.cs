using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public class EfGlAccountRepository : IGlAccountRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfGlAccountRepository(IDbContextFactory<BankingDbContext> factory) => _factory = factory;

    public IReadOnlyList<(string Number, string Name, string BalanceType)> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.GlAccounts
            .AsNoTracking()
            .OrderBy(e => e.AccountNumber)
            .ToList()
            .Select(e => (e.AccountNumber, e.Name, e.BalanceType))
            .ToList();
    }

    public void Save(string number, string name, string balanceType)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.GlAccounts.Find(number);
        if (entity == null)
            ctx.GlAccounts.Add(new GlAccountEntity { AccountNumber = number, Name = name, BalanceType = balanceType });
        else
        {
            entity.Name        = name;
            entity.BalanceType = balanceType;
        }
        ctx.SaveChanges();
    }

    public void Delete(string number)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.GlAccounts.Find(number);
        if (entity != null) { ctx.GlAccounts.Remove(entity); ctx.SaveChanges(); }
    }
}
