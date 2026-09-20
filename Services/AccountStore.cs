using RealCoreBanking.Core.Enums;
using RealCoreBanking.Core.Events;
using RealCoreBanking.Core.Models;
using RealCoreBanking.Data.Repositories;

namespace RealCoreBanking.Services;

public class AccountStore
{
    private readonly Dictionary<string, CurrentAccount>  _accounts         = new();
    private readonly Dictionary<string, InternalAccount> _internalAccounts = new();
    private readonly Dictionary<int, InterestRate>       _rates      = new();
    private readonly Dictionary<string, RateIndex>       _indexes    = new();
    private readonly Dictionary<string, Currency>        _currencies = new();
    private readonly List<string> _log = new();

    private readonly IInterestRateRepository?       _rateRepo;
    private readonly IRateIndexRepository?          _indexRepo;
    private readonly ICurrencyRepository?           _currencyRepo;
    private readonly IProductRepository?            _productRepo;
    private readonly IBalanceDefinitionRepository?  _balDefRepo;
    private readonly IFinancialIndicatorRepository? _fiRepo;
    private readonly IBalanceTypeRepository?        _btRepo;
    private readonly IGlAccountRepository?           _glRepo;
    private readonly ICurrentAccountRepository?     _accountRepo;
    private readonly IInternalAccountRepository?    _internalRepo;

    public Enterprise Enterprise { get; } = new Enterprise();

    public AccountStore(
        IInterestRateRepository?       rateRepo      = null,
        IRateIndexRepository?          indexRepo     = null,
        ICurrencyRepository?           currencyRepo  = null,
        IProductRepository?            productRepo   = null,
        IBalanceDefinitionRepository?  balDefRepo    = null,
        IFinancialIndicatorRepository? fiRepo        = null,
        IBalanceTypeRepository?        btRepo        = null,
        IGlAccountRepository?          glRepo        = null,
        ICurrentAccountRepository?     accountRepo   = null,
        IInternalAccountRepository?    internalRepo  = null)
    {
        _rateRepo     = rateRepo;
        _indexRepo    = indexRepo;
        _currencyRepo = currencyRepo;
        _productRepo  = productRepo;
        _balDefRepo   = balDefRepo;
        _fiRepo       = fiRepo;
        _btRepo       = btRepo;
        _glRepo       = glRepo;
        _accountRepo  = accountRepo;
        _internalRepo = internalRepo;

        // Restore persisted data from DB on startup
        if (_rateRepo      != null) foreach (var r in _rateRepo.GetAll())      _rates[r.Code]                    = r;
        if (_indexRepo     != null) foreach (var i in _indexRepo.GetAll())     _indexes[i.Code]                  = i;
        if (_accountRepo   != null) foreach (var a in _accountRepo.GetAll())   _accounts[a.AccountId]            = a;
        if (_internalRepo  != null) foreach (var a in _internalRepo.GetAll())  _internalAccounts[a.AccountId]    = a;

        // Load currencies — seed from SeedList if DB is empty
        if (_currencyRepo != null)
        {
            var dbCurrencies = _currencyRepo.GetAll();
            if (dbCurrencies.Any())
            {
                foreach (var c in dbCurrencies) _currencies[c.AlphaCode] = c;
            }
            else
            {
                foreach (var c in Currency.SeedList)
                {
                    _currencies[c.AlphaCode] = c;
                    _currencyRepo.Save(c);
                }
            }
        }

        // Load FinancialIndicators from DB — seed defaults if table is empty
        if (_fiRepo != null)
        {
            var dbFi = _fiRepo.GetAll();
            if (dbFi.Any())
            {
                foreach (var (code, desc) in dbFi)
                    FinancialIndicator.Register(code, desc);
            }
            else
            {
                var seeds = new[]
                {
                    ("001", "Основная сумма"),
                    ("002", "Проценты начисленные"),
                    ("003", "Начисленные убытки"),
                    ("004", "Внебаланс"),
                    ("005", "Блокировка"),
                    ("006", "Лимит овердрафта"),
                    ("007", "Сумма использованного овердрафта"),
                    ("008", "Штраф за просрочку"),
                    ("009", "Долг"),
                };
                foreach (var (code, desc) in seeds)
                {
                    FinancialIndicator.Register(code, desc);
                    _fiRepo.Save(code, desc);
                }
            }
        }

        // Load BalanceTypes from DB — seed defaults if table is empty
        if (_btRepo != null)
        {
            var dbBt = _btRepo.GetAll();
            if (dbBt.Any())
            {
                foreach (var (code, desc) in dbBt)
                    BalanceTypeDefinition.Register(code, desc);
            }
            else
            {
                var seeds = new[]
                {
                    ("01", "Увеличивает итог"),
                    ("02", "Уменьшает итог"),
                    ("03", "Нейтральный"),
                    ("04", "Блокирует итог"),
                    ("05", "Блокирует OTB"),
                };
                foreach (var (code, desc) in seeds)
                {
                    BalanceTypeDefinition.Register(code, desc);
                    _btRepo.Save(code, desc);
                }
            }
        }

        EventBus.Subscribe(e =>
        {
            lock (_log)
            {
                _log.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {e.EventType}");
                if (_log.Count > 300) _log.RemoveAt(300);
            }
        });
    }

    // ─── Accounts ────────────────────────────────────────────────────────────

    public IReadOnlyCollection<CurrentAccount> All => _accounts.Values;

    public void Add(CurrentAccount account)
    {
        _accounts[account.AccountId] = account;
        _accountRepo?.Save(account);
    }

    public void SaveAccount(CurrentAccount account)
    {
        _accounts[account.AccountId] = account;
        _accountRepo?.Save(account);
    }

    public void DeleteAccount(string accountId)
    {
        _accounts.Remove(accountId);
        _accountRepo?.Delete(accountId);
    }

    public CurrentAccount? Get(string id) => _accounts.TryGetValue(id, out var a) ? a : null;

    public TestSession TestSession { get; } = new();

    // ─── Products (DB only — no in-memory cache) ─────────────────────────────

    public void AddProduct(Product product)  => _productRepo?.Save(product);
    public void SaveProduct(Product product) => _productRepo?.Save(product);

    public Product? GetProduct(int code)            => _productRepo?.Get(code);
    public IReadOnlyCollection<Product> AllProducts => _productRepo?.GetAll() ?? [];

    public void DeleteProduct(int code) => _productRepo?.Delete(code);

    // ─── Interest Rates (persisted) ──────────────────────────────────────────

    public void AddInterestRate(InterestRate rate)
    {
        _rates[rate.Code] = rate;
        _rateRepo?.Save(rate);
    }

    public InterestRate? GetInterestRate(int code)        => _rates.TryGetValue(code, out var r) ? r : null;
    public IReadOnlyCollection<InterestRate> AllRates     => _rates.Values;

    public void DeleteInterestRate(int code)
    {
        _rates.Remove(code);
        _rateRepo?.Delete(code);
    }

    /// <summary>Persist an already-mutated in-memory rate (call after inline edits).</summary>
    public void PersistInterestRate(int code)
    {
        if (_rates.TryGetValue(code, out var r)) _rateRepo?.Save(r);
    }

    // ─── Rate Indexes (persisted) ────────────────────────────────────────────

    public void AddRateIndex(RateIndex index)
    {
        _indexes[index.Code] = index;
        _indexRepo?.Save(index);
    }

    public RateIndex? GetRateIndex(string code)         => _indexes.TryGetValue(code, out var i) ? i : null;
    public IReadOnlyCollection<RateIndex> AllIndexes    => _indexes.Values;

    public void DeleteRateIndex(string code)
    {
        _indexes.Remove(code);
        _indexRepo?.Delete(code);
    }

    /// <summary>Persist in-memory index metadata changes (call after inline edits).</summary>
    public void PersistRateIndex(string code)
    {
        if (_indexes.TryGetValue(code, out var idx)) _indexRepo?.Save(idx);
    }

    /// <summary>
    /// Add a value to an index — updates domain model AND persists to DB.
    /// Propagates the new rate to all linked InterestRate objects.
    /// </summary>
    public void AddIndexValue(string indexCode, RateValue value)
    {
        if (!_indexes.TryGetValue(indexCode, out var idx)) return;
        idx.RegisterRate(value);          // mutate domain model
        _indexRepo?.AddValue(indexCode, value);  // persist
        UpdateRatesForIndex(indexCode, value.Value);
    }

    /// <summary>Mark an index value as Voided — updates domain model AND persists.</summary>
    public void VoidIndexValue(string indexCode, DateTime valueDate)
    {
        if (!_indexes.TryGetValue(indexCode, out var idx)) return;
        idx.CancelRate(valueDate);        // mutate domain model
        _indexRepo?.VoidValue(indexCode, valueDate);  // persist
    }

    /// <summary>Recompute CurrentRate for all rates linked to this index and persist them.</summary>
    public void UpdateRatesForIndex(string indexCode, decimal indexValue)
    {
        foreach (var rate in _rates.Values.Where(r => r.RateIndexCode == indexCode))
        {
            rate.UpdateRate(indexValue);
            _rateRepo?.Save(rate);
        }
    }

    // ─── Currencies (persisted) ──────────────────────────────────────────────

    public IReadOnlyCollection<Currency> AllCurrencies => _currencies.Values;

    public void SaveCurrency(Currency currency)
    {
        _currencies[currency.AlphaCode] = currency;
        _currencyRepo?.Save(currency);
    }

    public Currency? GetCurrency(string alphaCode) =>
        _currencies.TryGetValue(alphaCode.ToUpper(), out var c) ? c : null;

    public Currency? GetCurrencyByNumericCode(string numericCode) =>
        _currencies.Values.FirstOrDefault(c => c.NumericCode == numericCode);

    public void DeleteCurrency(string alphaCode)
    {
        _currencies.Remove(alphaCode.ToUpper());
        _currencyRepo?.Delete(alphaCode);
    }

    // ─── Balance Definitions (DB only — no in-memory cache) ──────────────────

    public void AddBalanceDefinition(BalanceDefinition def) => _balDefRepo?.Save(def);

    public BalanceDefinition? GetBalanceDefinition(string id)               => _balDefRepo?.Get(id);
    public IReadOnlyCollection<BalanceDefinition> AllBalanceDefinitions     => _balDefRepo?.GetAll() ?? [];

    public void SaveBalanceDefinition(BalanceDefinition def)  => _balDefRepo?.Save(def);
    public void DeleteBalanceDefinition(string id)             => _balDefRepo?.Delete(id);

    public IReadOnlyList<(string Number, string Name, string BalanceType)> AllGlAccounts =>
        _glRepo?.GetAll() ?? [];

    // ─── Financial Indicators (persisted) ───────────────────────────────────

    public void SaveFinancialIndicator(string code, string description)
    {
        FinancialIndicator.Register(code, description);
        _fiRepo?.Save(code, description);
    }

    public void DeleteFinancialIndicator(string code)
    {
        FinancialIndicator.Unregister(code);
        _fiRepo?.Delete(code);
    }

    // ─── Balance Types (persisted) ───────────────────────────────────────────

    public void SaveBalanceType(string code, string description)
    {
        BalanceTypeDefinition.Register(code, description);
        _btRepo?.Save(code, description);
    }

    public void DeleteBalanceType(string code)
    {
        BalanceTypeDefinition.Unregister(code);
        _btRepo?.Delete(code);
    }

    // ─── Internal Accounts ───────────────────────────────────────────────────

    public IReadOnlyCollection<InternalAccount> AllInternalAccounts => _internalAccounts.Values;

    public void AddInternalAccount(InternalAccount account)
    {
        _internalAccounts[account.AccountId] = account;
        _internalRepo?.Save(account);
    }

    public void SaveInternalAccount(InternalAccount account)
    {
        _internalAccounts[account.AccountId] = account;
        _internalRepo?.Save(account);
    }

    public InternalAccount? GetInternalAccount(string id) =>
        _internalAccounts.TryGetValue(id, out var a) ? a : null;

    public void DeleteInternalAccount(string accountId)
    {
        _internalAccounts.Remove(accountId);
        _internalRepo?.Delete(accountId);
    }

    // ─── Event log ───────────────────────────────────────────────────────────

    public IReadOnlyList<string> RecentEvents
    {
        get { lock (_log) return _log.ToList().AsReadOnly(); }
    }
}
