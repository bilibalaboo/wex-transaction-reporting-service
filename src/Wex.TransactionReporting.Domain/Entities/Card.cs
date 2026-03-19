using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Errors;

namespace Wex.TransactionReporting.Domain.Entities;

public sealed class Card
{
    public Guid Id { get; private set; }
    public decimal CreditLimit { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Card() { }

    public static Result<Card> Create(decimal creditLimit)
    {
        if (creditLimit <= 0)
            return DomainErrors.Card.InvalidCreditLimit;

        return new Card
        {
            Id = Guid.NewGuid(),
            CreditLimit = creditLimit,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public Result<Transaction> RecordTransaction(
        string description,
        DateOnly transactionDate,
        decimal amountUsd,
        string idempotencyKey)
    {
        if (amountUsd <= 0)
            return DomainErrors.Transaction.InvalidAmount;

        return Transaction.Create(Id, description, transactionDate, amountUsd, idempotencyKey);
    }
}
