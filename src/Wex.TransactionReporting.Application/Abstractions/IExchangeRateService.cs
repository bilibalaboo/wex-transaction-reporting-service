using Wex.TransactionReporting.Domain.Common;

namespace Wex.TransactionReporting.Application.Abstractions;

public interface IExchangeRateService
{
    Task<Result<ExchangeRateResult>> GetForTransactionDateAsync(
        string currency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default);

    Task<Result<ExchangeRateResult>> GetLatestAsync(
        string currency,
        CancellationToken cancellationToken = default);
}
