namespace RealCoreBanking.Data.Repositories;

public interface IBalanceTypeRepository
{
    IReadOnlyList<(string Code, string Description)> GetAll();
    void Save(string code, string description);
    void Delete(string code);
}
