namespace Oks.Authentication.Jwt.Options;

public sealed class OksJwtOptions
{
    public string Issuer { get; set; } = "oks-auth";
    public string Audience { get; set; } = "oks-services";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
