using Wex.TransactionReporting.Domain.Common;

namespace Wex.TransactionReporting.Domain.Errors;

public static class DomainErrors
{
    public static class Card
    {
        public static readonly Error NotFound =
            new("Card.NotFound", "The specified card was not found.");

        public static readonly Error InvalidCreditLimit =
            new("Card.InvalidCreditLimit", "Credit limit must be greater than zero.");
    }

    public static class Transaction
    {
        public static readonly Error NotFound =
            new("Transaction.NotFound", "The specified transaction was not found.");

        public static readonly Error InvalidAmount =
            new("Transaction.InvalidAmount", "Transaction amount must be greater than zero.");

        public static readonly Error DuplicateIdempotencyKey =
            new("Transaction.DuplicateIdempotencyKey", "A transaction with this idempotency key already exists.");
    }

    public static class ExchangeRate
    {
        public static readonly Error NotFound =
            new("ExchangeRate.NotFound", "No exchange rate available for the specified currency within the required window.");
    }
}
