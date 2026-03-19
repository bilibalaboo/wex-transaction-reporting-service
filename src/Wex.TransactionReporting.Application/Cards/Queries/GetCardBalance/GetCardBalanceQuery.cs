namespace Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;

public sealed record GetCardBalanceQuery(Guid CardId, string Currency);
