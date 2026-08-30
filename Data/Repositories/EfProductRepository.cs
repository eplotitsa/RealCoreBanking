using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data.Repositories;

public class EfProductRepository : IProductRepository
{
    private readonly IDbContextFactory<BankingDbContext> _factory;
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = false };

    public EfProductRepository(IDbContextFactory<BankingDbContext> factory) => _factory = factory;

    public IReadOnlyList<Product> GetAll()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.Products.AsNoTracking()
            .Select(e => e.Data)
            .ToList()
            .Select(json => JsonSerializer.Deserialize<Product>(json, _json)!)
            .Where(p => p != null)
            .ToList();
    }

    public Product? Get(int code)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.Products.AsNoTracking().FirstOrDefault(e => e.Code == code);
        return entity == null ? null : JsonSerializer.Deserialize<Product>(entity.Data, _json);
    }

    public void Save(Product product)
    {
        using var ctx = _factory.CreateDbContext();
        var json   = JsonSerializer.Serialize(product, _json);
        var entity = ctx.Products.Find(product.Code);
        if (entity == null)
            ctx.Products.Add(new ProductEntity { Code = product.Code, Data = json });
        else
            entity.Data = json;
        ctx.SaveChanges();
    }

    public void Delete(int code)
    {
        using var ctx = _factory.CreateDbContext();
        var entity = ctx.Products.Find(code);
        if (entity != null) { ctx.Products.Remove(entity); ctx.SaveChanges(); }
    }
}
