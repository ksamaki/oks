namespace Oks.Persistence.EfCore.Options;

public sealed class OksRepositoryLoggingOptions
{
    /// <summary>
    /// Repository READ/WRITE loglama aktif mi?
    /// </summary>
    public bool Enabled { get; set; } = true;
}