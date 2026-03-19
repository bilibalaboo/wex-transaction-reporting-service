using Microsoft.EntityFrameworkCore;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
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
        => await dbContext.SaveChangesAsync(cancellationToken);
}
