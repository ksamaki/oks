using Microsoft.AspNetCore.Authorization;
using Oks.Authentication.Core.Constants;

namespace Oks.Authentication.AspNetCore.Authorization;

public sealed class OksPermissionAuthorizationHandler : AuthorizationHandler<OksPermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OksPermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(c => c.Type == OksClaimTypes.Permission && c.Value == requirement.Permission);
        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
