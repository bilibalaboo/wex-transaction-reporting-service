using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Wex.TransactionReporting.Infrastructure.Observability;

[ExcludeFromCodeCoverage]
public static class AppMeter
{
    public const string Name = "Wex.TransactionReporting";
    private static readonly Meter Instance = new(Name, "1.0.0");

    public static readonly Histogram<double> TreasuryApiRequestDuration =
        Instance.CreateHistogram<double>("treasury_api.request.duration", "ms",
            "Duration of Treasury API requests. Alert if p95 > 5000ms.");

    public static readonly Counter<long> ExchangeRateCacheHits =
        Instance.CreateCounter<long>("exchange_rate.cache.hits", "hits",
            "Number of exchange rate cache hits.");

    public static readonly Counter<long> ExchangeRateCacheMisses =
        Instance.CreateCounter<long>("exchange_rate.cache.misses", "misses",
            "Number of exchange rate cache misses.");

    public static readonly Counter<long> TransactionsCreated =
        Instance.CreateCounter<long>("transactions.created", "transactions",
            "Number of purchase transactions successfully created.");
}
