using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();
    Product? Get(int code);
    void Save(Product product);
    void Delete(int code);
}
