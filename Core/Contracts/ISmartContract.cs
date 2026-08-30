using RealCoreBanking.Core.Models;

namespace RealCoreBanking.Core.Contracts;

public interface ISmartContract
{
    string Name { get; }

    void BeforeChange(CurrentAccount account) { }
    void Execute(CurrentAccount account);
    void AfterChange(CurrentAccount account) { }
    void OnDayOpen(CurrentAccount account) { }
    void OnDayClose(CurrentAccount account) { }
}
