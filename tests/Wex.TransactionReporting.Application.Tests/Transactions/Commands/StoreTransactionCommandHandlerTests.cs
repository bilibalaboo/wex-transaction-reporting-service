using FluentAssertions;
using NSubstitute;
using Wex.TransactionReporting.Application.Transactions.Commands.StoreTransaction;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Tests.Transactions.Commands;

public sealed class StoreTransactionCommandHandlerTests
{
    private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly StoreTransactionCommandHandler _handler;

    public StoreTransactionCommandHandlerTests()
    {
        _handler = new StoreTransactionCommandHandler(_cardRepository, _transactionRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsTransactionId()
    {
        var card = Card.Create(1000m).Value!;
        var cardId = card.Id;
        _cardRepository.GetByIdAsync(cardId, Arg.Any<CancellationToken>()).Returns(card);

        var command = new StoreTransactionCommand(cardId, "Coffee", new DateOnly(2024, 6, 15), 25m, Guid.NewGuid().ToString());

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidCommand_SavesTransaction()
    {
        var card = Card.Create(1000m).Value!;
        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);

        var command = new StoreTransactionCommand(card.Id, "Coffee", new DateOnly(2024, 6, 15), 25m, Guid.NewGuid().ToString());

        await _handler.Handle(command);

        await _transactionRepository.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _transactionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ReturnsFailure()
    {
        _cardRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var command = new StoreTransactionCommand(Guid.NewGuid(), "Coffee", new DateOnly(2024, 6, 15), 25m, Guid.NewGuid().ToString());

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Card.NotFound");
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_DoesNotSave()
    {
        _cardRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var command = new StoreTransactionCommand(Guid.NewGuid(), "Coffee", new DateOnly(2024, 6, 15), 25m, Guid.NewGuid().ToString());

        await _handler.Handle(command);

        await _transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _transactionRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_WithInvalidAmount_ReturnsFailure(decimal amount)
    {
        var card = Card.Create(1000m).Value!;
        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);

        var command = new StoreTransactionCommand(card.Id, "Coffee", new DateOnly(2024, 6, 15), amount, Guid.NewGuid().ToString());

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Transaction.InvalidAmount");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_WithInvalidAmount_DoesNotSave(decimal amount)
    {
        var card = Card.Create(1000m).Value!;
        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);

        var command = new StoreTransactionCommand(card.Id, "Coffee", new DateOnly(2024, 6, 15), amount, Guid.NewGuid().ToString());

        await _handler.Handle(command);

        await _transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _transactionRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
