using Microsoft.AspNetCore.Builder;
using Oks.Web.Validation.Filters;

namespace Oks.Web.Validation;

public static class OksMinimalApiValidationRouteGroupBuilderExtensions
{
    public static RouteGroupBuilder AddOksValidation(this RouteGroupBuilder routeGroupBuilder)
        => routeGroupBuilder.AddEndpointFilter<OksMinimalApiValidationFilter>();
}
