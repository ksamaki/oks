using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oks.Web.Validation.Filters;

namespace Oks.Web.Validation;

public static class OksMinimalApiValidationRouteGroupBuilderExtensions
{
    public static RouteGroupBuilder AddOksValidation(this RouteGroupBuilder routeGroupBuilder)
        => routeGroupBuilder.AddEndpointFilter<OksMinimalApiValidationFilter>();
}
