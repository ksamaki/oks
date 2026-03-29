using Oks.Authentication.Abstractions.Models;

namespace Oks.Authentication.Abstractions.Contracts;

public interface IAuthenticationService
{
    Task<TokenPair> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<TokenPair> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
