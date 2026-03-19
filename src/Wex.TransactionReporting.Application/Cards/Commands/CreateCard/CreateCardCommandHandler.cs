using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Cards.Commands.CreateCard;

public sealed class CreateCardCommandHandler(ICardRepository cardRepository)
{
    public async ValueTask<Result<Guid>> Handle(
        CreateCardCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = Card.Create(command.CreditLimit);
        if (result.IsFailure)
            return result.Error!;

        await cardRepository.AddAsync(result.Value!, cancellationToken);
        await cardRepository.SaveChangesAsync(cancellationToken);

        return result.Value!.Id;
    }
}
