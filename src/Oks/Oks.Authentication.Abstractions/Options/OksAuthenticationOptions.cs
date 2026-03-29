namespace Oks.Authentication.Abstractions.Options;

public sealed class OksAuthenticationOptions
{
    public string DefaultSchema { get; set; } = "oks_auth";
    public bool EnableSeed { get; set; }
    public bool EnableAutoMigrate { get; set; }
    public bool EnableFailedLoginTracking { get; set; } = true;
    public bool EnableSecurityEventPersistence { get; set; } = true;
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);
    public int MaxFailedAttempts { get; set; } = 5;
}
