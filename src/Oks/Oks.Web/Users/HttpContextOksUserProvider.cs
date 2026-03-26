using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Oks.Persistence.Abstractions;

namespace Oks.Web.Users;

public sealed class HttpContextOksUserProvider : IOksUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextOksUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserIdentifier()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub")
               ?? user.Identity?.Name;
    }
}
