namespace Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;

public sealed record GetTransactionInCurrencyQuery(Guid TransactionId, string Currency);
