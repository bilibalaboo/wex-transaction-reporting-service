namespace Wex.TransactionReporting.Application.Abstractions;

public sealed record ExchangeRateResult(
    string Currency,
    DateOnly EffectiveDate,
    decimal Rate);
