namespace Oks.Authentication.OpenIddict.Options;

public sealed class OksOpenIddictOptions
{
    public bool EnableAuthorizationCodeFlow { get; set; } = true;
    public bool EnableClientCredentialsFlow { get; set; } = true;
    public bool EnableRefreshTokenFlow { get; set; } = true;
    public IReadOnlyCollection<string> DefaultScopes { get; set; } = ["openid", "profile", "oks_api"];
}
