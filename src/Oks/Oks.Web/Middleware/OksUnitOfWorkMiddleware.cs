using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Web.Abstractions.Attributes;

namespace Oks.Web.Middleware;

public class OksUnitOfWorkMiddleware
{
    private readonly RequestDelegate _next;

    public OksUnitOfWorkMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        await _next(context);

        // 5xx cevaplarda commit etme
        if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            return;

        var endpoint = context.GetEndpoint();
        if (endpoint is null)
            return;

        // MVC controller endpoint'lerinde commit işi OksUnitOfWorkFilter'da
        if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
            return;

        // Endpoint seviyesinde skip metadata'sı varsa commit etme
        if (endpoint.Metadata.GetMetadata<OksSkipTransactionAttribute>() is not null)
            return;

        await unitOfWork.SaveChangesAsync(context.RequestAborted);
    }
}
