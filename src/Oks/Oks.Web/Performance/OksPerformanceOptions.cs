namespace Oks.Web.Performance;

public sealed class OksPerformanceOptions
{
    /// <summary>
    /// Performance ölçüm mekanizması global olarak aktif mi?
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Varsayılan milisaniye cinsinden threshold.
    /// Örn: 1000 = 1 saniye.
    /// </summary>
    public int DefaultThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Threshold aşılırsa exception fırlatılsın mı?
    /// </summary>
    public bool ThrowOnSlowRequest { get; set; } = true;
}