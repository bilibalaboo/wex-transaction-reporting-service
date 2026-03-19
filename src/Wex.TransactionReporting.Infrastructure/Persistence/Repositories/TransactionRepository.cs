using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Repositories;
using Wex.TransactionReporting.Infrastructure.Observability;

namespace Wex.TransactionReporting.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(
    AppDbContext dbContext,
    ILogger<TransactionRepository> logger) : ITransactionRepository
{
    public async ValueTask<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Transactions.FindAsync([id], cancellationToken);

    public async ValueTask<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        => await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);

    public async ValueTask<decimal> GetTotalSpendByCardIdAsync(Guid cardId, CancellationToken cancellationToken = default)
        => await dbContext.Transactions
            .Where(t => t.CardId == cardId)
            .SumAsync(t => t.AmountUsd, cancellationToken);

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => await dbContext.Transactions.AddAsync(transaction, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var newTransactions = dbContext.ChangeTracker
            .Entries<Transaction>()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var t in newTransactions)
        {
            logger.TransactionCreated(t.Id, t.CardId);
            AppMeter.TransactionsCreated.Add(1, new TagList { { "card_id", t.CardId.ToString() } });
        }
    }
}
