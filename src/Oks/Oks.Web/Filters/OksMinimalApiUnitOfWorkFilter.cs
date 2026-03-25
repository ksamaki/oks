using Microsoft.AspNetCore.Http;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Web.Abstractions.Attributes;

namespace Oks.Web.Filters;

public sealed class OksMinimalApiUnitOfWorkFilter : IEndpointFilter
{
    private readonly IUnitOfWork _unitOfWork;

    public OksMinimalApiUnitOfWorkFilter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var response = await next(context);

        if (ShouldSkipTransaction(context.HttpContext))
            return response;

        if (context.HttpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            return response;

        await _unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
        return response;
    }

    private static bool ShouldSkipTransaction(HttpContext httpContext)
        => httpContext.GetEndpoint()?.Metadata.GetMetadata<OksSkipTransactionAttribute>() is not null;
}
