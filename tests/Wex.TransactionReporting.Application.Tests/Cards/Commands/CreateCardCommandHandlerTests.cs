using FluentAssertions;
using NSubstitute;
using Wex.TransactionReporting.Application.Cards.Commands.CreateCard;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Tests.Cards.Commands;

public sealed class CreateCardCommandHandlerTests
{
    private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
    private readonly CreateCardCommandHandler _handler;

    public CreateCardCommandHandlerTests()
    {
        _handler = new CreateCardCommandHandler(_cardRepository);
    }

    [Fact]
    public async Task Handle_WithValidCreditLimit_ReturnsCardId()
    {
        var command = new CreateCardCommand(500m);

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidCreditLimit_SavesCard()
    {
        var command = new CreateCardCommand(500m);

        await _handler.Handle(command);

        await _cardRepository.Received(1).AddAsync(Arg.Any<Domain.Entities.Card>(), Arg.Any<CancellationToken>());
        await _cardRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WithInvalidCreditLimit_ReturnsFailure(decimal creditLimit)
    {
        var command = new CreateCardCommand(creditLimit);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Card.InvalidCreditLimit");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WithInvalidCreditLimit_DoesNotSave(decimal creditLimit)
    {
        var command = new CreateCardCommand(creditLimit);

        await _handler.Handle(command);

        await _cardRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.Card>(), Arg.Any<CancellationToken>());
        await _cardRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
