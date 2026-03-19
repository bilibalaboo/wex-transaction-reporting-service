using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Domain.Common;

namespace Wex.TransactionReporting.Infrastructure.ExchangeRates;

public sealed class CachedExchangeRateService(
    TreasuryExchangeRateService treasury,
    IDistributedCache cache) : IExchangeRateService
{
    public async Task<Result<ExchangeRateResult>> GetForTransactionDateAsync(
        string currency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default)
    {
        var key = $"treasury:{currency}:{transactionDate:yyyy-MM-dd}";

        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize(cached, InfrastructureJsonContext.Default.ExchangeRateResult)!;

        var result = await treasury.GetForTransactionDateAsync(currency, transactionDate, cancellationToken);

        if (result.IsSuccess)
            await cache.SetStringAsync(key,
                JsonSerializer.Serialize(result.Value, InfrastructureJsonContext.Default.ExchangeRateResult),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) },
                cancellationToken);

        return result;
    }

    public async Task<Result<ExchangeRateResult>> GetLatestAsync(
        string currency,
        CancellationToken cancellationToken = default)
    {
        var key = $"treasury:{currency}:latest";

        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize(cached, InfrastructureJsonContext.Default.ExchangeRateResult)!;

        var result = await treasury.GetLatestAsync(currency, cancellationToken);

        if (result.IsSuccess)
            await cache.SetStringAsync(key,
                JsonSerializer.Serialize(result.Value, InfrastructureJsonContext.Default.ExchangeRateResult),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) },
                cancellationToken);

        return result;
    }
}
