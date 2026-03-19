namespace Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;

public sealed record CardBalanceResponse(
    Guid CardId,
    decimal CreditLimitUsd,
    decimal AvailableBalanceUsd,
    string TargetCurrency,
    decimal ExchangeRateUsed,
    DateOnly ExchangeRateDate,
    decimal AvailableBalanceConverted);
