using FluentValidation;

namespace Wex.TransactionReporting.Api.Models;

public sealed class StoreTransactionRequestValidator : AbstractValidator<StoreTransactionRequest>
{
    public StoreTransactionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.TransactionDate)
            .NotEmpty();

        RuleFor(x => x.AmountUsd)
            .GreaterThan(0);
    }
}
