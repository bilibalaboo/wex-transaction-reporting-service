using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Errors;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Transactions.Commands.StoreTransaction;

public sealed class StoreTransactionCommandHandler(
    ICardRepository cardRepository,
    ITransactionRepository transactionRepository)
{
    public async ValueTask<Result<Guid>> Handle(
        StoreTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetByIdAsync(command.CardId, cancellationToken);
        if (card is null)
            return DomainErrors.Card.NotFound;

        var result = card.RecordTransaction(
            command.Description,
            command.TransactionDate,
            command.AmountUsd,
            command.IdempotencyKey);

        if (result.IsFailure)
            return result.Error!;

        await transactionRepository.AddAsync(result.Value!, cancellationToken);
        await transactionRepository.SaveChangesAsync(cancellationToken);

        return result.Value!.Id;
    }
}
