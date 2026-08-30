using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public class EfBalanceDefinitionRepository : IBalanceDefinitionRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;

    public EfBalanceDefinitionRepository(IDbContextFactory<BankingDbContext> factory) => _factory = factory;

    public IReadOnlyList<BalanceDefinition> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.BalanceDefinitions.AsNoTracking()
            .Select(e => new BalanceDefinition
            {
                Id                     = e.Id,
                Name                   = e.Name,
                BalanceTypePrefix      = e.BalanceTypePrefix,
                FinancialIndicatorCode = e.FinancialIndicatorCode,
            })
            .ToList();
    }

    public BalanceDefinition? Get(string id)
    {
        using var ctx = _factory.CreateDbContext();
        var e = ctx.BalanceDefinitions.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return e == null ? null : new BalanceDefinition
        {
            Id                     = e.Id,
            Name                   = e.Name,
            BalanceTypePrefix      = e.BalanceTypePrefix,
            FinancialIndicatorCode = e.FinancialIndicatorCode,
        };
    }

    public void Save(BalanceDefinition def)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.BalanceDefinitions.Find(def.Id);
        if (entity == null)
        {
            ctx.BalanceDefinitions.Add(new BalanceDefinitionEntity
            {
                Id                     = def.Id,
                Name                   = def.Name,
                BalanceTypePrefix      = def.BalanceTypePrefix,
                FinancialIndicatorCode = def.FinancialIndicatorCode,
            });
        }
        else
        {
            entity.Name                   = def.Name;
            entity.BalanceTypePrefix      = def.BalanceTypePrefix;
            entity.FinancialIndicatorCode = def.FinancialIndicatorCode;
        }
        ctx.SaveChanges();
    }

    public void Delete(string id)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.BalanceDefinitions.Find(id);
        if (entity != null) { ctx.BalanceDefinitions.Remove(entity); ctx.SaveChanges(); }
    }
}
