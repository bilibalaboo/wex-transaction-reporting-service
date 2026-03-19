using Microsoft.EntityFrameworkCore;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Infrastructure.Persistence.Repositories;

public sealed class CardRepository(AppDbContext dbContext) : ICardRepository
{
    public async ValueTask<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Cards.FindAsync([id], cancellationToken);

    public async Task AddAsync(Card card, CancellationToken cancellationToken = default)
        => await dbContext.Cards.AddAsync(card, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
