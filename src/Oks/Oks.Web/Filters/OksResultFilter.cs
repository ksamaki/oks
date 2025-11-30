using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oks.Shared.Results;

namespace Oks.Web.Filters;

public class OksResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult &&
            objectResult.Value is Result result)
        {
            // HTTP status code'u ResultStatus'tan al
            var statusCode = (int)result.Status;

            context.HttpContext.Response.StatusCode = statusCode;

            // Success / Fail durumuna göre body aynı kalabilir;
            // zaten Result / DataResult serialized olacak.
            context.Result = new ObjectResult(result)
            {
                StatusCode = statusCode
            };
        }

        await next();
    }
}