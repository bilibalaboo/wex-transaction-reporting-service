using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Infrastructure.Observability;

namespace Wex.TransactionReporting.Infrastructure.ExchangeRates;

public sealed class CachedExchangeRateService(
    TreasuryExchangeRateService treasury,
    IDistributedCache cache,
    ILogger<CachedExchangeRateService> logger) : IExchangeRateService
{
    public async Task<Result<ExchangeRateResult>> GetForTransactionDateAsync(
        string currency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default)
    {
        var key = $"treasury:{currency}:{transactionDate:yyyy-MM-dd}";
        return await GetOrFetch(key, currency,
            () => treasury.GetForTransactionDateAsync(currency, transactionDate, cancellationToken),
            TimeSpan.FromHours(6),
            cancellationToken);
    }

    public async Task<Result<ExchangeRateResult>> GetLatestAsync(
        string currency,
        CancellationToken cancellationToken = default)
    {
        var key = $"treasury:{currency}:latest";
        return await GetOrFetch(key, currency,
            () => treasury.GetLatestAsync(currency, cancellationToken),
            TimeSpan.FromMinutes(15),
            cancellationToken);
    }

    private async Task<Result<ExchangeRateResult>> GetOrFetch(
        string key,
        string currency,
        Func<Task<Result<ExchangeRateResult>>> fetch,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            logger.CacheHit(key);
            AppMeter.ExchangeRateCacheHits.Add(1, new TagList { { "currency", currency } });
            return JsonSerializer.Deserialize(cached, InfrastructureJsonContext.Default.ExchangeRateResult)!;
        }

        logger.CacheMiss(key);
        AppMeter.ExchangeRateCacheMisses.Add(1, new TagList { { "currency", currency } });

        var result = await fetch();

        if (result.IsSuccess)
            await cache.SetStringAsync(key,
                JsonSerializer.Serialize(result.Value, InfrastructureJsonContext.Default.ExchangeRateResult),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);

        return result;
    }
}
