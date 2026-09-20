using Microsoft.EntityFrameworkCore;
using RealCoreBanking.Data.Entities;

namespace RealCoreBanking.Data;

/// <summary>
/// EF Core DbContext for RealCoreBanking persistence.
/// To switch DBMS: change the provider in Program.cs (UseSqlite → UseNpgsql / UseSqlServer).
/// Schema is created automatically via EnsureCreated() on first run.
/// </summary>
public class BankingDbContext : DbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }

    public DbSet<RateIndexEntity>           RateIndexes           { get; set; }
    public DbSet<RateValueEntity>           RateValues            { get; set; }
    public DbSet<InterestRateEntity>        InterestRates         { get; set; }
    public DbSet<CurrencyEntity>            Currencies            { get; set; }
    public DbSet<ProductEntity>             Products              { get; set; }
    public DbSet<BalanceDefinitionEntity>   BalanceDefinitions    { get; set; }
    public DbSet<FinancialIndicatorEntity>  FinancialIndicators   { get; set; }
    public DbSet<BalanceTypeEntity>         BalanceTypes          { get; set; }
    public DbSet<GlAccountEntity>           GlAccounts            { get; set; }
    public DbSet<CurrentAccountEntity>      CurrentAccounts       { get; set; }
    public DbSet<InternalAccountEntity>     InternalAccounts      { get; set; }
    public DbSet<TransactionEntity>         Transactions          { get; set; }
    public DbSet<PostingEntity>             Postings              { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // RateIndex ← RateValue (1:many, cascade delete)
        mb.Entity<RateValueEntity>()
          .HasOne(v => v.Index)
          .WithMany(i => i.Values)
          .HasForeignKey(v => v.IndexCode)
          .OnDelete(DeleteBehavior.Cascade);

        // Composite index for fast value lookups
        mb.Entity<RateValueEntity>()
          .HasIndex(v => new { v.IndexCode, v.ValueDate });

        // InterestRates: optional FK to RateIndexes (no cascade — index may be deleted independently)
        mb.Entity<InterestRateEntity>()
          .HasIndex(r => r.RateIndexCode);

        // Transactions → Postings (1:many, cascade delete)
        mb.Entity<PostingEntity>()
          .HasOne(p => p.Transaction)
          .WithMany(t => t.Postings)
          .HasForeignKey(p => p.TxId)
          .OnDelete(DeleteBehavior.Cascade);

        // Fast lookups by account and by date
        mb.Entity<TransactionEntity>()
          .HasIndex(t => t.AccountId);
        mb.Entity<TransactionEntity>()
          .HasIndex(t => new { t.AccountId, t.Date });
        mb.Entity<PostingEntity>()
          .HasIndex(p => p.TxId);
    }
}
