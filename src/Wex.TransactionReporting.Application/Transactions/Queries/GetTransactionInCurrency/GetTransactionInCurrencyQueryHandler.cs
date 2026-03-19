using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Errors;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;

public sealed class GetTransactionInCurrencyQueryHandler(
    ITransactionRepository transactionRepository,
    IExchangeRateService exchangeRateService)
{
    public async ValueTask<Result<TransactionInCurrencyResponse>> Handle(
        GetTransactionInCurrencyQuery query,
        CancellationToken cancellationToken = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(query.TransactionId, cancellationToken);
        if (transaction is null)
            return DomainErrors.Transaction.NotFound;

        var rateResult = await exchangeRateService.GetForTransactionDateAsync(
            query.Currency,
            transaction.TransactionDate,
            cancellationToken);

        if (rateResult.IsFailure)
            return rateResult.Error!;

        var rate = rateResult.Value!;

        return new TransactionInCurrencyResponse(
            Id: transaction.Id,
            Description: transaction.Description,
            TransactionDate: transaction.TransactionDate,
            OriginalAmountUsd: transaction.AmountUsd,
            TargetCurrency: rate.Currency,
            ExchangeRateUsed: rate.Rate,
            ExchangeRateDate: rate.EffectiveDate,
            ConvertedAmount: transaction.AmountUsd * rate.Rate);
    }
}
