using FluentValidation;

namespace Wex.TransactionReporting.Application.Transactions.Commands.StoreTransaction;

public sealed class StoreTransactionCommandValidator : AbstractValidator<StoreTransactionCommand>
{
    public StoreTransactionCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty()
            .WithMessage("CardId is required.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(255);

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Transaction date is required.");

        RuleFor(x => x.AmountUsd)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Idempotency-Key header is required.");
    }
}
