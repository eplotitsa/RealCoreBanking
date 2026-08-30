using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

public interface ICurrencyRepository
{
    IReadOnlyList<Currency> GetAll();
    void Save(Currency currency);
    void Delete(string alphaCode);
}
