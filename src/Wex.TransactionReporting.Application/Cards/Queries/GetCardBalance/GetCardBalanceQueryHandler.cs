using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Errors;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;

public sealed class GetCardBalanceQueryHandler(
    ICardRepository cardRepository,
    ITransactionRepository transactionRepository,
    IExchangeRateService exchangeRateService)
{
    public async ValueTask<Result<CardBalanceResponse>> Handle(
        GetCardBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        var card = await cardRepository.GetByIdAsync(query.CardId, cancellationToken);
        if (card is null)
            return DomainErrors.Card.NotFound;

        var totalSpendUsd = await transactionRepository
            .GetTotalSpendByCardIdAsync(query.CardId, cancellationToken);

        var availableBalanceUsd = card.CreditLimit - totalSpendUsd;

        var rateResult = await exchangeRateService.GetLatestAsync(query.Currency, cancellationToken);
        if (rateResult.IsFailure)
            return rateResult.Error!;

        var rate = rateResult.Value!;

        return new CardBalanceResponse(
            CardId: card.Id,
            CreditLimitUsd: card.CreditLimit,
            AvailableBalanceUsd: availableBalanceUsd,
            TargetCurrency: rate.Currency,
            ExchangeRateUsed: rate.Rate,
            ExchangeRateDate: rate.EffectiveDate,
            AvailableBalanceConverted: availableBalanceUsd * rate.Rate);
    }
}
