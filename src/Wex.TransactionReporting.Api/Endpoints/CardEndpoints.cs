using FluentValidation;
using Wex.TransactionReporting.Api.Extensions;
using Wex.TransactionReporting.Api.Filters;
using Wex.TransactionReporting.Api.Models;
using Wex.TransactionReporting.Application.Cards.Commands.CreateCard;
using Wex.TransactionReporting.Application.Cards.Queries.GetCardBalance;
using Wex.TransactionReporting.Application.Transactions.Commands.StoreTransaction;

namespace Wex.TransactionReporting.Api.Endpoints;

public static class CardEndpoints
{
    public static IEndpointRouteBuilder MapCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cards");

        group.MapPost("/", async (
            CreateCardCommand command,
            CreateCardCommandHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/cards/{result.Value}", result.Value)
                : result.Error!.ToProblem();
        })
        .AddEndpointFilter<ValidationFilter<CreateCardCommand>>();

        group.MapPost("/{cardId:guid}/transactions", async (
            Guid cardId,
            StoreTransactionRequest request,
            HttpContext httpContext,
            StoreTransactionCommandHandler handler,
            CancellationToken ct) =>
        {
            var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].ToString();
            var command = new StoreTransactionCommand(
                cardId,
                request.Description,
                request.TransactionDate,
                request.AmountUsd,
                idempotencyKey);

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/transactions/{result.Value}", result.Value)
                : result.Error!.ToProblem();
        })
        .AddEndpointFilter<IdempotencyFilter>()
        .AddEndpointFilter<ValidationFilter<StoreTransactionRequest>>();

        group.MapGet("/{cardId:guid}/balance", async (
            Guid cardId,
            string currency,
            GetCardBalanceQueryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetCardBalanceQuery(cardId, currency), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error!.ToProblem();
        });

        return app;
    }
}
