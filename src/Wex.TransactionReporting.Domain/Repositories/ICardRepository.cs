using Wex.TransactionReporting.Domain.Entities;

namespace Wex.TransactionReporting.Domain.Repositories;

public interface ICardRepository
{
    ValueTask<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
