using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public sealed class EfCurrencyRepository : ICurrencyRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfCurrencyRepository(IDbContextFactory<BankingDbContext> factory)
        => _factory = factory;

    public IReadOnlyList<Currency> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.Currencies
            .AsNoTracking()
            .ToList()
            .Select(ToDomain)
            .ToList();
    }

    public void Save(Currency currency)
    {
        using var ctx = _factory.CreateDbContext();
        var entity   = ToEntity(currency);
        var existing = ctx.Currencies.Find(currency.AlphaCode);
        if (existing is null) ctx.Currencies.Add(entity);
        else ctx.Entry(existing).CurrentValues.SetValues(entity);
        ctx.SaveChanges();
    }

    public void Delete(string alphaCode)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.Currencies.Find(alphaCode);
        if (entity is not null) { ctx.Currencies.Remove(entity); ctx.SaveChanges(); }
    }

    private static Currency ToDomain(CurrencyEntity e) => new()
    {
        AlphaCode    = e.AlphaCode,
        NumericCode  = e.NumericCode,
        Name         = e.Name,
        DecimalPlaces= e.DecimalPlaces,
        RoundingRule = Enum.TryParse<RoundingRule>(e.RoundingRule, out var rr)
                       ? rr : RoundingRule.AwayFromZero,
    };

    private static CurrencyEntity ToEntity(Currency c) => new()
    {
        AlphaCode    = c.AlphaCode,
        NumericCode  = c.NumericCode,
        Name         = c.Name,
        DecimalPlaces= c.DecimalPlaces,
        RoundingRule = c.RoundingRule.ToString(),
    };
}
