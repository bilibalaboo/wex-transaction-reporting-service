using FluentAssertions;
using NSubstitute;
using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;
using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Errors;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Tests.Cards.Queries;

public sealed class GetCardBalanceQueryHandlerTests
{
    private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IExchangeRateService _exchangeRateService = Substitute.For<IExchangeRateService>();
    private readonly GetCardBalanceQueryHandler _handler;

    public GetCardBalanceQueryHandlerTests()
    {
        _handler = new GetCardBalanceQueryHandler(_cardRepository, _transactionRepository, _exchangeRateService);
    }

    [Fact]
    public async Task Handle_WithNoSpend_ReturnsFullCreditLimitAsAvailableBalance()
    {
        var card = Card.Create(1000m).Value!;
        var rate = new ExchangeRateResult("EUR", new DateOnly(2024, 6, 15), 0.92m);

        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);
        _transactionRepository.GetTotalSpendByCardIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(0m);
        _exchangeRateService.GetLatestAsync("EUR", Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetCardBalanceQuery(card.Id, "EUR");

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AvailableBalanceUsd.Should().Be(1000m);
        result.Value.AvailableBalanceConverted.Should().Be(920m);
    }

    [Fact]
    public async Task Handle_WithSpend_DeductsFromCreditLimit()
    {
        var card = Card.Create(1000m).Value!;
        var rate = new ExchangeRateResult("EUR", new DateOnly(2024, 6, 15), 1.0m);

        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);
        _transactionRepository.GetTotalSpendByCardIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(300m);
        _exchangeRateService.GetLatestAsync("EUR", Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetCardBalanceQuery(card.Id, "EUR");

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AvailableBalanceUsd.Should().Be(700m);
    }

    [Fact]
    public async Task Handle_MapsAllResponseFields()
    {
        var card = Card.Create(2000m).Value!;
        var rateDate = new DateOnly(2024, 6, 15);
        var rate = new ExchangeRateResult("GBP", rateDate, 0.79m);

        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);
        _transactionRepository.GetTotalSpendByCardIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(500m);
        _exchangeRateService.GetLatestAsync("GBP", Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetCardBalanceQuery(card.Id, "GBP");

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CardId.Should().Be(card.Id);
        result.Value.CreditLimitUsd.Should().Be(2000m);
        result.Value.AvailableBalanceUsd.Should().Be(1500m);
        result.Value.TargetCurrency.Should().Be("GBP");
        result.Value.ExchangeRateUsed.Should().Be(0.79m);
        result.Value.ExchangeRateDate.Should().Be(rateDate);
        result.Value.AvailableBalanceConverted.Should().Be(1500m * 0.79m);
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ReturnsFailure()
    {
        _cardRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var query = new GetCardBalanceQuery(Guid.NewGuid(), "EUR");

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Card.NotFound");
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_DoesNotCallExchangeRateService()
    {
        _cardRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var query = new GetCardBalanceQuery(Guid.NewGuid(), "EUR");

        await _handler.Handle(query);

        await _exchangeRateService.DidNotReceive().GetLatestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRateNotFound_ReturnsFailure()
    {
        var card = Card.Create(1000m).Value!;

        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);
        _transactionRepository.GetTotalSpendByCardIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(0m);
        _exchangeRateService.GetLatestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Failure(DomainErrors.ExchangeRate.NotFound));

        var query = new GetCardBalanceQuery(card.Id, "EUR");

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("ExchangeRate.NotFound");
    }

    [Fact]
    public async Task Handle_UsesLatestRate_NotTransactionDateRate()
    {
        var card = Card.Create(1000m).Value!;
        var rate = new ExchangeRateResult("EUR", new DateOnly(2024, 6, 15), 0.95m);

        _cardRepository.GetByIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(card);
        _transactionRepository.GetTotalSpendByCardIdAsync(card.Id, Arg.Any<CancellationToken>()).Returns(0m);
        _exchangeRateService.GetLatestAsync("EUR", Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetCardBalanceQuery(card.Id, "EUR");

        await _handler.Handle(query);

        await _exchangeRateService.Received(1).GetLatestAsync("EUR", Arg.Any<CancellationToken>());
        await _exchangeRateService.DidNotReceive()
            .GetForTransactionDateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }
}
