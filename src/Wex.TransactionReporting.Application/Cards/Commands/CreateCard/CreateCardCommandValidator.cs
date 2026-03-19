using FluentValidation;

namespace Wex.TransactionReporting.Application.Cards.Commands.CreateCard;

public sealed class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(x => x.CreditLimit)
            .GreaterThan(0)
            .WithMessage("Credit limit must be greater than zero.");
    }
}
