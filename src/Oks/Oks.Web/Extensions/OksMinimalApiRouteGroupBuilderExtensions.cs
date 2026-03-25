using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oks.Web.Filters;

namespace Oks.Web.Extensions;

public static class OksMinimalApiRouteGroupBuilderExtensions
{
    public static RouteGroupBuilder AddOksUnitOfWork(this RouteGroupBuilder routeGroupBuilder)
        => routeGroupBuilder.AddEndpointFilter<OksMinimalApiUnitOfWorkFilter>();

    public static RouteGroupBuilder AddOksResultWrapping(this RouteGroupBuilder routeGroupBuilder)
        => routeGroupBuilder.AddEndpointFilter<OksMinimalApiResultFilter>();

    public static RouteGroupBuilder AddOksRateLimiting(this RouteGroupBuilder routeGroupBuilder)
        => routeGroupBuilder.AddEndpointFilter<OksMinimalApiRateLimitFilter>();

    public static RouteGroupBuilder AddOksPerformance(this RouteGroupBuilder routeGroupBuilder)
        => routeGroupBuilder.AddEndpointFilter<OksMinimalApiPerformanceFilter>();
}
