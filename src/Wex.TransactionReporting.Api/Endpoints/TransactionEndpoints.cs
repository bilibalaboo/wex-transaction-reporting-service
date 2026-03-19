using Wex.TransactionReporting.Api.Extensions;
using Wex.TransactionReporting.Application.Transactions.Queries.GetTransactionInCurrency;

namespace Wex.TransactionReporting.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions");

        group.MapGet("/{transactionId:guid}", async (
            Guid transactionId,
            string currency,
            GetTransactionInCurrencyQueryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetTransactionInCurrencyQuery(transactionId, currency), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error!.ToProblem();
        });

        return app;
    }
}
