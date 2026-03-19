namespace Wex.TransactionReporting.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid CardId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateOnly TransactionDate { get; private set; }
    public decimal AmountUsd { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    
    private Transaction() { }

    internal static Transaction Create(
        Guid cardId,
        string description,
        DateOnly transactionDate,
        decimal amountUsd,
        string idempotencyKey) => new()
    {
        Id = Guid.NewGuid(),
        CardId = cardId,
        Description = description,
        TransactionDate = transactionDate,
        AmountUsd = amountUsd,
        IdempotencyKey = idempotencyKey,
        CreatedAt = DateTimeOffset.UtcNow
    };
}
