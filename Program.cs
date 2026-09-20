using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
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
builder.Services.AddSingleton<IGlAccountRepository,          EfGlAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository,        EfTransactionRepository>();
builder.Services.AddSingleton<ICurrentAccountRepository,     EfCurrentAccountRepository>();
builder.Services.AddSingleton<IInternalAccountRepository,    EfInternalAccountRepository>();

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
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "GlAccounts" (
            "AccountNumber" TEXT NOT NULL CONSTRAINT "PK_GlAccounts" PRIMARY KEY,
            "Name"          TEXT NOT NULL DEFAULT '',
            "BalanceType"   TEXT NOT NULL DEFAULT 'Assets'
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "CurrentAccounts" (
            "AccountId" TEXT NOT NULL CONSTRAINT "PK_CurrentAccounts" PRIMARY KEY,
            "Data"      TEXT NOT NULL DEFAULT ''
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "InternalAccounts" (
            "AccountId" TEXT NOT NULL CONSTRAINT "PK_InternalAccounts" PRIMARY KEY,
            "Data"      TEXT NOT NULL DEFAULT ''
        )
        """);

    // Transactions and Postings — extracted from account JSON blobs
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Transactions" (
            "TxId"           TEXT    NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
            "AccountId"      TEXT    NOT NULL DEFAULT '',
            "Date"           TEXT    NOT NULL DEFAULT '',
            "Amount"         TEXT    NOT NULL DEFAULT '0',
            "Currency"       TEXT    NOT NULL DEFAULT '',
            "BalanceAddress" INTEGER NOT NULL DEFAULT 0,
            "Type"           TEXT    NOT NULL DEFAULT 'Financial',
            "Status"         TEXT    NOT NULL DEFAULT 'Posted',
            "Description"    TEXT    NOT NULL DEFAULT ''
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_Transactions_AccountId"
            ON "Transactions" ("AccountId")
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_Transactions_AccountId_Date"
            ON "Transactions" ("AccountId", "Date")
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Postings" (
            "PostingId"   TEXT    NOT NULL CONSTRAINT "PK_Postings" PRIMARY KEY,
            "TxId"        TEXT    NOT NULL DEFAULT '',
            "Date"        TEXT    NOT NULL DEFAULT '',
            "DrAddress"   INTEGER NOT NULL DEFAULT 0,
            "CrAddress"   INTEGER NOT NULL DEFAULT 0,
            "Amount"      TEXT    NOT NULL DEFAULT '0',
            "Currency"    TEXT    NOT NULL DEFAULT '',
            "Description" TEXT    NOT NULL DEFAULT '',
            CONSTRAINT "FK_Postings_Transactions_TxId"
                FOREIGN KEY ("TxId") REFERENCES "Transactions" ("TxId") ON DELETE CASCADE
        )
        """);
    ctx.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_Postings_TxId"
            ON "Postings" ("TxId")
        """);

    // Migrate existing transactions out of account JSON blobs (one-time, idempotent).
    MigrateTransactionsFromBlobs(ctx);
}

// Force AccountStore initialization so FinancialIndicator and BalanceTypeDefinition
// are seeded from DB before any request arrives.
_ = app.Services.GetRequiredService<AccountStore>();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

// ── Local helper: one-time migration of Transactions out of JSON blobs ────────
// Uses Microsoft.Data.Sqlite directly — avoids EF Core quirks with raw scalar queries.
static void MigrateTransactionsFromBlobs(RealCoreBanking.Data.BankingDbContext ctx)
{
    // Get the connection string from EF Core context
    var connStr = ctx.Database.GetConnectionString()!;
    using var conn = new SqliteConnection(connStr);
    conn.Open();

    // Collect all account rows from both tables
    var rows = new List<(string AccountId, string Data, string Table)>();
    foreach (var table in new[] { "CurrentAccounts", "InternalAccounts" })
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT AccountId, Data FROM \"{table}\"";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add((r.GetString(0), r.GetString(1), table));
    }

    int migrated = 0;
    foreach (var (accountId, data, table) in rows)
    {
        try
        {
            var node = JsonNode.Parse(data);
            var txArray = node?["Transactions"]?.AsArray();
            if (txArray == null || txArray.Count == 0) continue;

            using var txBegin = conn.BeginTransaction();
            foreach (var txNode in txArray)
            {
                if (txNode == null) continue;
                var txId = txNode["TxId"]?.GetValue<string>() ?? "";
                if (string.IsNullOrEmpty(txId)) continue;

                // Check if already migrated
                using var existsCmd = conn.CreateCommand();
                existsCmd.Transaction = txBegin;
                existsCmd.CommandText = "SELECT COUNT(*) FROM Transactions WHERE TxId = @id";
                existsCmd.Parameters.AddWithValue("@id", txId);
                var exists = (long)(existsCmd.ExecuteScalar() ?? 0L);
                if (exists > 0) continue;

                // Insert transaction
                using var insTx = conn.CreateCommand();
                insTx.Transaction = txBegin;
                insTx.CommandText = """
                    INSERT OR IGNORE INTO Transactions
                        (TxId, AccountId, Date, Amount, Currency, BalanceAddress, Type, Status, Description)
                    VALUES (@txId, @acct, @date, @amt, @cur, @bal, @type, @status, @desc)
                    """;
                insTx.Parameters.AddWithValue("@txId",   txId);
                insTx.Parameters.AddWithValue("@acct",   accountId);
                insTx.Parameters.AddWithValue("@date",   txNode["Date"]?.GetValue<string>()    ?? "");
                insTx.Parameters.AddWithValue("@amt",    txNode["Amount"]?.ToString()          ?? "0");
                insTx.Parameters.AddWithValue("@cur",    txNode["Currency"]?.GetValue<string>() ?? "");
                insTx.Parameters.AddWithValue("@bal",    txNode["BalanceAddress"]?.GetValue<long>() ?? 0L);
                insTx.Parameters.AddWithValue("@type",   txNode["Type"]?.GetValue<string>()   ?? "Financial");
                insTx.Parameters.AddWithValue("@status", txNode["Status"]?.GetValue<string>() ?? "Posted");
                insTx.Parameters.AddWithValue("@desc",   txNode["Description"]?.GetValue<string>() ?? "");
                insTx.ExecuteNonQuery();

                // Insert postings
                var postings = txNode["Postings"]?.AsArray();
                if (postings != null)
                {
                    foreach (var p in postings)
                    {
                        if (p == null) continue;
                        var pid = p["PostingId"]?.GetValue<string>() ?? "";
                        if (string.IsNullOrEmpty(pid)) continue;

                        using var insP = conn.CreateCommand();
                        insP.Transaction = txBegin;
                        insP.CommandText = """
                            INSERT OR IGNORE INTO Postings
                                (PostingId, TxId, Date, DrAddress, CrAddress, Amount, Currency, Description)
                            VALUES (@pid, @txId, @date, @dr, @cr, @amt, @cur, @desc)
                            """;
                        insP.Parameters.AddWithValue("@pid",  pid);
                        insP.Parameters.AddWithValue("@txId", txId);
                        insP.Parameters.AddWithValue("@date", p["Date"]?.GetValue<string>()       ?? "");
                        insP.Parameters.AddWithValue("@dr",   p["DrAddress"]?.GetValue<long>()    ?? 0L);
                        insP.Parameters.AddWithValue("@cr",   p["CrAddress"]?.GetValue<long>()    ?? 0L);
                        insP.Parameters.AddWithValue("@amt",  p["Amount"]?.ToString()             ?? "0");
                        insP.Parameters.AddWithValue("@cur",  p["Currency"]?.GetValue<string>()   ?? "");
                        insP.Parameters.AddWithValue("@desc", p["Description"]?.GetValue<string>() ?? "");
                        insP.ExecuteNonQuery();
                    }
                }

                migrated++;
            }

            // Strip Transactions[] from the JSON blob
            node!["Transactions"] = new JsonArray();
            var newJson = node.ToJsonString();

            using var upd = conn.CreateCommand();
            upd.Transaction = txBegin;
            upd.CommandText = $"UPDATE \"{table}\" SET Data = @json WHERE AccountId = @id";
            upd.Parameters.AddWithValue("@json", newJson);
            upd.Parameters.AddWithValue("@id",   accountId);
            upd.ExecuteNonQuery();

            txBegin.Commit();
        }
        catch
        {
            // Skip malformed rows — migration is idempotent, will retry next startup
        }
    }
}

// Helper projection type (kept for potential future use)
class AccountRow
{
    public string AccountId { get; set; } = "";
    public string Data      { get; set; } = "";
}
