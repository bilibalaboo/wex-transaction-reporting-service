using Wex.TransactionReporting.Domain.Common;

namespace Wex.TransactionReporting.Application.Abstractions;

public interface IExchangeRateService
{
    ValueTask<Result<ExchangeRateResult>> GetForTransactionDateAsync(
        string currency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default);

    ValueTask<Result<ExchangeRateResult>> GetLatestAsync(
        string currency,
        CancellationToken cancellationToken = default);
}
