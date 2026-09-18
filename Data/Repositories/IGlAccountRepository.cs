namespace RealCoreBanking.Data.Repositories;

public interface IGlAccountRepository
{
    IReadOnlyList<(string Number, string Name, string BalanceType)> GetAll();
    void Save(string number, string name, string balanceType);
    void Delete(string number);
}
