namespace Wex.TransactionReporting.Api.Models;

public sealed record StoreTransactionRequest(
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd);
