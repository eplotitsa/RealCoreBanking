using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Data;
using RealCoreBanking.Data.Repositories;
using RealCoreBanking.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

// ── Database ──────────────────────────────────────────────────────────────────
// SQLite: data stored in banking.db next to the executable.
// To switch DBMS → change one line here:
//   PostgreSQL : options.UseNpgsql("Host=...;Database=banking")
//   SQL Server : options.UseSqlServer("Server=...;Database=banking")
var dbPath = Path.Combine(AppContext.BaseDirectory, "banking.db");
builder.Services.AddDbContextFactory<BankingDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ── Repositories (singleton-safe via IDbContextFactory) ───────────────────────
builder.Services.AddSingleton<IInterestRateRepository,       EfInterestRateRepository>();
builder.Services.AddSingleton<IRateIndexRepository,          EfRateIndexRepository>();
builder.Services.AddSingleton<ICurrencyRepository,           EfCurrencyRepository>();
builder.Services.AddSingleton<IProductRepository,            EfProductRepository>();
builder.Services.AddSingleton<IBalanceDefinitionRepository,  EfBalanceDefinitionRepository>();
builder.Services.AddSingleton<IFinancialIndicatorRepository, EfFinancialIndicatorRepository>();
builder.Services.AddSingleton<IBalanceTypeRepository,        EfBalanceTypeRepository>();

// ── AccountStore — receives repositories, loads from DB in constructor ─────────
builder.Services.AddSingleton<AccountStore>();

var app = builder.Build();

// ── Ensure DB schema exists (creates banking.db on first run) ─────────────────
var dbFactory = app.Services.GetRequiredService<IDbContextFactory<BankingDbContext>>();
using (var ctx = dbFactory.CreateDbContext())
{
    ctx.Database.EnsureCreated();
    // Add tables introduced after initial DB creation (EnsureCreated won't alter existing DBs)
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Currencies" (
            "AlphaCode"     TEXT NOT NULL CONSTRAINT "PK_Currencies" PRIMARY KEY,
            "NumericCode"   TEXT NOT NULL DEFAULT '',
            "Name"          TEXT NOT NULL DEFAULT '',
            "DecimalPlaces" INTEGER NOT NULL DEFAULT 2,
            "RoundingRule"  TEXT NOT NULL DEFAULT 'AwayFromZero'
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Products" (
            "Code" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY,
            "Data" TEXT NOT NULL DEFAULT ''
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "BalanceDefinitions" (
            "Id"                     TEXT NOT NULL CONSTRAINT "PK_BalanceDefinitions" PRIMARY KEY,
            "Name"                   TEXT NOT NULL DEFAULT '',
            "BalanceTypePrefix"      INTEGER NOT NULL DEFAULT 1,
            "FinancialIndicatorCode" TEXT NOT NULL DEFAULT '001'
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "FinancialIndicators" (
            "Code"        TEXT NOT NULL CONSTRAINT "PK_FinancialIndicators" PRIMARY KEY,
            "Description" TEXT NOT NULL DEFAULT ''
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "BalanceTypes" (
            "Code"        TEXT NOT NULL CONSTRAINT "PK_BalanceTypes" PRIMARY KEY,
            "Description" TEXT NOT NULL DEFAULT ''
        )
        """);
}

// Force AccountStore initialization so FinancialIndicator and BalanceTypeDefinition
// are seeded from DB before any request arrives.
_ = app.Services.GetRequiredService<AccountStore>();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
