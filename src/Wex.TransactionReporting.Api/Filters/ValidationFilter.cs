using FluentValidation;

namespace Wex.TransactionReporting.Api.Filters;

public sealed class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        T? argument = default;
        foreach (var arg in context.Arguments)
        {
            if (arg is T typed)
            {
                argument = typed;
                break;
            }
        }

        if (argument is null)
            return await next(context);

        var validation = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        return await next(context);
    }
}
