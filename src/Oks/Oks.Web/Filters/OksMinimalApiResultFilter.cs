using Microsoft.AspNetCore.Http;
using Oks.Shared.Results;

namespace Oks.Web.Filters;

public sealed class OksMinimalApiResultFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var response = await next(context);

        if (response is null)
            return response;

        if (response is IResult)
            return response;

        if (response is Result result)
        {
            context.HttpContext.Response.StatusCode = (int)result.Status;
            return Results.Json(result, statusCode: (int)result.Status);
        }

        var wrapped = DataResult<object?>.Ok(response);
        context.HttpContext.Response.StatusCode = (int)wrapped.Status;
        return Results.Json(wrapped, statusCode: (int)wrapped.Status);
    }
}
