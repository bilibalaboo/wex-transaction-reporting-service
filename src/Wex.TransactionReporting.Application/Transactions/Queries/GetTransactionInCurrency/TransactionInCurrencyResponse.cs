namespace Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;

public sealed record TransactionInCurrencyResponse(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal OriginalAmountUsd,
    string TargetCurrency,
    decimal ExchangeRateUsed,
    DateOnly ExchangeRateDate,
    decimal ConvertedAmount);
