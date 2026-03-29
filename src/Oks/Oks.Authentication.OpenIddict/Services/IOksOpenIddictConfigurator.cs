using Microsoft.Extensions.DependencyInjection;
using Oks.Authentication.OpenIddict.Options;

namespace Oks.Authentication.OpenIddict.Services;

/// <summary>
/// Adapter abstraction to keep OpenIddict dependency optional.
/// Host application can implement this interface in a project that references OpenIddict package.
/// </summary>
public interface IOksOpenIddictConfigurator
{
    void Configure(IServiceCollection services, OksOpenIddictOptions options);
}
