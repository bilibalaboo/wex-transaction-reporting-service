using Wex.TransactionReporting.Domain.Repositories;

namespace Wex.TransactionReporting.Api.Filters;

public sealed class IdempotencyFilter(ITransactionRepository transactionRepository) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var key = context.HttpContext.Request.Headers["Idempotency-Key"].ToString();

        if (string.IsNullOrWhiteSpace(key))
            return Results.Problem(
                detail: "Idempotency-Key header is required.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "MissingIdempotencyKey");

        var existing = await transactionRepository.GetByIdempotencyKeyAsync(
            key, context.HttpContext.RequestAborted);

        if (existing is not null)
            return Results.Created($"/transactions/{existing.Id}", existing.Id);

        return await next(context);
    }
}
