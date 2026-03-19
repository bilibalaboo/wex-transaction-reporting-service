namespace Wex.TransactionReporting.Application.Transactions.Commands.StoreTransaction;

public sealed record StoreTransactionCommand(
    Guid CardId,
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd,
    string IdempotencyKey);
