using Oks.Authentication.Abstractions.Models;

namespace Oks.Authentication.Abstractions.Contracts;

public interface IClientStore
{
    Task<ClientContext?> GetActiveClientAsync(string clientCode, CancellationToken cancellationToken = default);
    Task<bool> ValidateSecretAsync(string clientCode, string? providedSecret, CancellationToken cancellationToken = default);
}
