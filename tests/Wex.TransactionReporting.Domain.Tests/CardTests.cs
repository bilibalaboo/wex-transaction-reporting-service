using FluentAssertions;
using Wex.TransactionReporting.Domain.Entities;

namespace Wex.TransactionReporting.Domain.Tests;

public sealed class CardTests
{
    [Fact]
    public void Create_WithValidCreditLimit_ReturnsSuccess()
    {
        var result = Card.Create(1000m);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CreditLimit.Should().Be(1000m);
        result.Value.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public void Create_WithInvalidCreditLimit_ReturnsFailure(decimal creditLimit)
    {
        var result = Card.Create(creditLimit);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Card.InvalidCreditLimit");
    }

    [Fact]
    public void RecordTransaction_WithValidAmount_ReturnsTransaction()
    {
        var card = Card.Create(1000m).Value!;
        var date = new DateOnly(2024, 6, 15);

        var result = card.RecordTransaction("Coffee", date, 25m, Guid.NewGuid().ToString());

        result.IsSuccess.Should().BeTrue();
        result.Value!.AmountUsd.Should().Be(25m);
        result.Value.Description.Should().Be("Coffee");
        result.Value.TransactionDate.Should().Be(date);
        result.Value.CardId.Should().Be(card.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void RecordTransaction_WithInvalidAmount_ReturnsFailure(decimal amount)
    {
        var card = Card.Create(1000m).Value!;

        var result = card.RecordTransaction("Test", new DateOnly(2024, 6, 15), amount, Guid.NewGuid().ToString());

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Transaction.InvalidAmount");
    }

    [Fact]
    public void RecordTransaction_AssignsUniqueIdPerTransaction()
    {
        var card = Card.Create(1000m).Value!;
        var date = new DateOnly(2024, 6, 15);

        var t1 = card.RecordTransaction("A", date, 10m, Guid.NewGuid().ToString()).Value!;
        var t2 = card.RecordTransaction("B", date, 20m, Guid.NewGuid().ToString()).Value!;

        t1.Id.Should().NotBe(t2.Id);
    }
}
