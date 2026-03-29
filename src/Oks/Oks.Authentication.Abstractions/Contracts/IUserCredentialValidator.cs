using Oks.Authentication.Abstractions.Models;

namespace Oks.Authentication.Abstractions.Contracts;

public interface IUserCredentialValidator
{
    Task<AuthenticatedUser?> ValidateAsync(string userName, string password, string clientCode, CancellationToken cancellationToken = default);
}
