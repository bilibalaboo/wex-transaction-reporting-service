using FluentAssertions;
using NSubstitute;
using Wex.TransactionReporting.Application.Abstractions;
using Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;
using Wex.TransactionReporting.Domain.Common;
using Wex.TransactionReporting.Domain.Entities;
using Wex.TransactionReporting.Domain.Errors;
using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Application.Tests.Transactions.Queries;

public sealed class GetTransactionInCurrencyQueryHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IExchangeRateService _exchangeRateService = Substitute.For<IExchangeRateService>();
    private readonly GetTransactionInCurrencyQueryHandler _handler;

    public GetTransactionInCurrencyQueryHandlerTests()
    {
        _handler = new GetTransactionInCurrencyQueryHandler(_transactionRepository, _exchangeRateService);
    }

    private static Transaction CreateTransaction(decimal amountUsd = 100m)
    {
        var card = Card.Create(1000m).Value!;
        return card.RecordTransaction("Test", new DateOnly(2024, 6, 15), amountUsd, Guid.NewGuid().ToString()).Value!;
    }

    [Fact]
    public async Task Handle_WithValidTransactionAndRate_ReturnsConvertedResponse()
    {
        var transaction = CreateTransaction(100m);
        var rate = new ExchangeRateResult("EUR", new DateOnly(2024, 6, 10), 0.92m);

        _transactionRepository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        _exchangeRateService.GetForTransactionDateAsync("EUR", transaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetTransactionInCurrencyQuery(transaction.Id, "EUR");

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ConvertedAmount.Should().Be(92m);
        result.Value.ExchangeRateUsed.Should().Be(0.92m);
        result.Value.TargetCurrency.Should().Be("EUR");
        result.Value.OriginalAmountUsd.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_WithValidTransaction_MapsAllFields()
    {
        var transaction = CreateTransaction(50m);
        var rateDate = new DateOnly(2024, 6, 10);
        var rate = new ExchangeRateResult("GBP", rateDate, 0.79m);

        _transactionRepository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        _exchangeRateService.GetForTransactionDateAsync("GBP", transaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetTransactionInCurrencyQuery(transaction.Id, "GBP");

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(transaction.Id);
        result.Value.Description.Should().Be(transaction.Description);
        result.Value.TransactionDate.Should().Be(transaction.TransactionDate);
        result.Value.ExchangeRateDate.Should().Be(rateDate);
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ReturnsFailure()
    {
        _transactionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var query = new GetTransactionInCurrencyQuery(Guid.NewGuid(), "EUR");

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Transaction.NotFound");
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_DoesNotCallExchangeRateService()
    {
        _transactionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var query = new GetTransactionInCurrencyQuery(Guid.NewGuid(), "EUR");

        await _handler.Handle(query);

        await _exchangeRateService.DidNotReceive()
            .GetForTransactionDateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRateNotFound_ReturnsFailure()
    {
        var transaction = CreateTransaction(100m);

        _transactionRepository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        _exchangeRateService.GetForTransactionDateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Failure(DomainErrors.ExchangeRate.NotFound));

        var query = new GetTransactionInCurrencyQuery(transaction.Id, "EUR");

        var result = await _handler.Handle(query);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("ExchangeRate.NotFound");
    }

    [Fact]
    public async Task Handle_ConvertedAmount_IsProductOfAmountAndRate()
    {
        var transaction = CreateTransaction(200m);
        var rate = new ExchangeRateResult("JPY", new DateOnly(2024, 6, 10), 157.5m);

        _transactionRepository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        _exchangeRateService.GetForTransactionDateAsync("JPY", transaction.TransactionDate, Arg.Any<CancellationToken>())
            .Returns(Result<ExchangeRateResult>.Success(rate));

        var query = new GetTransactionInCurrencyQuery(transaction.Id, "JPY");

        var result = await _handler.Handle(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ConvertedAmount.Should().Be(200m * 157.5m);
    }
}
