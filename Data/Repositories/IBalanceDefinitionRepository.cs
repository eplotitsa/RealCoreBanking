using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Data.Repositories;

public interface IBalanceDefinitionRepository
{
    IReadOnlyList<BalanceDefinition> GetAll();
    BalanceDefinition? Get(string id);
    void Save(BalanceDefinition def);
    void Delete(string id);
}
