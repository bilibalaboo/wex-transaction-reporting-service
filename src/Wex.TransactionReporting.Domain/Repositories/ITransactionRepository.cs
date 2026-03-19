using Wex.TransactionReporting.Domain.Entities;

namespace Wex.TransactionReporting.Domain.Repositories;

public interface ITransactionRepository
{
    ValueTask<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    ValueTask<decimal> GetTotalSpendByCardIdAsync(Guid cardId, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
