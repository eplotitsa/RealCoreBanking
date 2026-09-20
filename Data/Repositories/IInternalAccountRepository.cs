using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

public interface IInternalAccountRepository
{
    IReadOnlyList<InternalAccount> GetAll();
    InternalAccount? Get(string accountId);
    void Save(InternalAccount account);
    void Delete(string accountId);
}
