using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

/// <summary>EF Core implementation of IRateIndexRepository. Safe for singleton injection via IDbContextFactory.</summary>
public sealed class EfRateIndexRepository : IRateIndexRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfRateIndexRepository(IDbContextFactory<BankingDbContext> factory)
        => _factory = factory;

    public IReadOnlyList<RateIndex> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.RateIndexes
            .Include(i => i.Values)
            .AsNoTracking()
            .ToList()
            .Select(RateMapper.ToDomain)
            .ToList();
    }

    public void Save(RateIndex index)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = RateMapper.ToEntity(index);
        var existing = ctx.RateIndexes.Find(index.Code);
        if (existing is null)
            ctx.RateIndexes.Add(entity);
        else
            ctx.Entry(existing).CurrentValues.SetValues(entity);
        ctx.SaveChanges();
    }

    public void Delete(string code)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.RateIndexes.Find(code);
        if (entity is not null) { ctx.RateIndexes.Remove(entity); ctx.SaveChanges(); }
    }

    public void AddValue(string indexCode, RateValue value)
    {
        using var ctx = _factory.CreateDbContext();

        // supersede any active value on the same date (mirrors RateIndex.RegisterRate logic)
        var clash = ctx.RateValues.FirstOrDefault(v =>
            v.IndexCode == indexCode &&
            v.ValueDate == value.ValueDate &&
            v.Status == "Active");
        if (clash is not null) clash.Status = "Superseded";

        ctx.RateValues.Add(RateMapper.ToEntity(value, indexCode));
        ctx.SaveChanges();
    }

    public void VoidValue(string indexCode, DateTime valueDate)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.RateValues.FirstOrDefault(v =>
            v.IndexCode == indexCode &&
            v.ValueDate == valueDate &&
            v.Status == "Active");
        if (entity is not null) { entity.Status = "Voided"; ctx.SaveChanges(); }
    }
}
