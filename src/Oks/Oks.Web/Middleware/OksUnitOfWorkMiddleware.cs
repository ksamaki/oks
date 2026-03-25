using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Web.Abstractions.Attributes;
using System.Reflection;

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

        var endpoint = ResolveEndpoint(context);
        if (endpoint is null)
            return;

        // MVC controller endpoint'lerinde commit işi OksUnitOfWorkFilter'da
        if (HasMetadata(endpoint, typeof(ControllerActionDescriptor)))
            return;

        // Endpoint seviyesinde skip metadata'sı varsa commit etme
        if (HasMetadata(endpoint, typeof(OksSkipTransactionAttribute)))
            return;

        await unitOfWork.SaveChangesAsync(context.RequestAborted);
    }

    private static object? ResolveEndpoint(HttpContext context)
    {
        var extensionsType = Type.GetType(
            "Microsoft.AspNetCore.Http.EndpointHttpContextExtensions, Microsoft.AspNetCore.Http.Abstractions");
        var getEndpointMethod = extensionsType?.GetMethod(
            "GetEndpoint",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            new[] { typeof(HttpContext) },
            modifiers: null);

        if (getEndpointMethod is not null)
            return getEndpointMethod.Invoke(null, new object[] { context });

        foreach (var feature in context.Features)
        {
            if (feature.Key.FullName != "Microsoft.AspNetCore.Http.Features.IEndpointFeature")
                continue;

            return feature.Value?.GetType().GetProperty("Endpoint")?.GetValue(feature.Value);
        }

        return null;
    }

    private static bool HasMetadata(object endpoint, Type metadataType)
    {
        var metadata = endpoint.GetType().GetProperty("Metadata")?.GetValue(endpoint);
        if (metadata is null)
            return false;

        var getMetadata = metadata.GetType().GetMethods()
            .FirstOrDefault(m =>
                m.Name == "GetMetadata" &&
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 0);

        if (getMetadata is null)
            return false;

        return getMetadata.MakeGenericMethod(metadataType).Invoke(metadata, null) is not null;
    }
}
