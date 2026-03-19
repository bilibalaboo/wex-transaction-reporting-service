using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Infrastructure.Persistence.Configurations;

namespace Wex.TransactionReporting.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CardConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
    }
}
