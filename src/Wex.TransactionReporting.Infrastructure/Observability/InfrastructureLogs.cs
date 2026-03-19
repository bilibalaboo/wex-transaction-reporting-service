using Microsoft.Extensions.Logging;

namespace Wex.TransactionReporting.Infrastructure.Observability;

internal static partial class TreasuryLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching Treasury exchange rate for {Currency}")]
    internal static partial void FetchingRate(this ILogger logger, string currency);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "No Treasury exchange rate found for {Currency}")]
    internal static partial void RateNotFound(this ILogger logger, string currency);

    [LoggerMessage(Level = LogLevel.Error, Message = "Treasury API request failed for {Currency} after {ElapsedMs}ms")]
    internal static partial void RequestFailed(this ILogger logger, string currency, double elapsedMs, Exception ex);
}

internal static partial class CacheLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Exchange rate cache hit for key {CacheKey}")]
    internal static partial void CacheHit(this ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Exchange rate cache miss for key {CacheKey}")]
    internal static partial void CacheMiss(this ILogger logger, string cacheKey);
}

internal static partial class TransactionLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction {TransactionId} created for card {CardId}")]
    internal static partial void TransactionCreated(this ILogger logger, Guid transactionId, Guid cardId);
}
