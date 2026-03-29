using Oks.Authentication.Abstractions.Models;

namespace Oks.Authentication.Abstractions.Contracts;

public interface ITokenIssuer
{
    Task<TokenPair> IssueAsync(AuthenticatedUser user, ClientContext client, Guid sessionId, CancellationToken cancellationToken = default);
}
