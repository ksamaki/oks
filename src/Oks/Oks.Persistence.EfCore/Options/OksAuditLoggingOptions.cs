namespace Oks.Persistence.EfCore.Options;

public sealed class OksAuditLoggingOptions
{
    /// <summary>
    /// Audit loglama aktif mi?
    /// </summary>
    public bool Enabled { get; set; } = true;
}