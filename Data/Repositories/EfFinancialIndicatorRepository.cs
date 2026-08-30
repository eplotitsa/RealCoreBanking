using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public class EfFinancialIndicatorRepository : IFinancialIndicatorRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfFinancialIndicatorRepository(IDbContextFactory<BankingDbContext> factory)
        => _factory = factory;

    public IReadOnlyList<(string Code, string Description)> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.FinancialIndicators
            .AsNoTracking()
            .OrderBy(e => e.Code)
            .ToList()
            .Select(e => (e.Code, e.Description))
            .ToList();
    }

    public void Save(string code, string description)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.FinancialIndicators.Find(code);
        if (entity == null)
            ctx.FinancialIndicators.Add(new FinancialIndicatorEntity { Code = code, Description = description });
        else
            entity.Description = description;
        ctx.SaveChanges();
    }

    public void Delete(string code)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.FinancialIndicators.Find(code);
        if (entity != null) { ctx.FinancialIndicators.Remove(entity); ctx.SaveChanges(); }
    }
}
