using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

public interface ICurrentAccountRepository
{
    IReadOnlyList<CurrentAccount> GetAll();
    CurrentAccount? Get(string accountId);
    void Save(CurrentAccount account);
    void Delete(string accountId);
}
