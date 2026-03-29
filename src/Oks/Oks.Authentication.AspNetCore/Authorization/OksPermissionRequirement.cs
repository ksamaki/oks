using Microsoft.AspNetCore.Authorization;

namespace Oks.Authentication.AspNetCore.Authorization;

public sealed class OksPermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
