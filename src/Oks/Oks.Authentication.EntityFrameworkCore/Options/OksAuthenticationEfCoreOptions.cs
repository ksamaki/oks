namespace Oks.Authentication.EntityFrameworkCore.Options;

public sealed class OksAuthenticationEfCoreOptions
{
    public string Schema { get; set; } = "oks_auth";
    public bool AutoMigrate { get; set; }
}
