using Wex.TransactionReporting.Domain.Common;

namespace Wex.TransactionReporting.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblem(this Error error) =>
        error.Code switch
        {
            var c when c.EndsWith("NotFound") =>
                Results.Problem(detail: error.Description, statusCode: StatusCodes.Status404NotFound, title: error.Code),
            var c when c.EndsWith("DuplicateIdempotencyKey") =>
                Results.Problem(detail: error.Description, statusCode: StatusCodes.Status409Conflict, title: error.Code),
            _ =>
                Results.Problem(detail: error.Description, statusCode: StatusCodes.Status422UnprocessableEntity, title: error.Code)
        };
}
