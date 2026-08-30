using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

/// <summary>EF Core implementation of IInterestRateRepository. Safe for singleton injection via IDbContextFactory.</summary>
public sealed class EfInterestRateRepository : IInterestRateRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfInterestRateRepository(IDbContextFactory<BankingDbContext> factory)
        => _factory = factory;

    public IReadOnlyList<InterestRate> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.InterestRates
            .AsNoTracking()
            .ToList()
            .Select(RateMapper.ToDomain)
            .ToList();
    }

    public void Save(InterestRate rate)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = RateMapper.ToEntity(rate);
        var existing = ctx.InterestRates.Find(rate.Code);
        if (existing is null)
            ctx.InterestRates.Add(entity);
        else
            ctx.Entry(existing).CurrentValues.SetValues(entity);
        ctx.SaveChanges();
    }

    public void Delete(int code)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.InterestRates.Find(code);
        if (entity is not null) { ctx.InterestRates.Remove(entity); ctx.SaveChanges(); }
    }
}
