using Oks.Logging.Abstractions.Enums;

namespace Oks.Logging.Abstractions.Models;

/// <summary>
/// OKS içindeki tüm loglama noktalarının konuştuğu ortak model.
/// Hangi tabloya gideceğine IOksLogWriter implementasyonu karar verir.
/// </summary>
public sealed class OksLogEntry
{
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public OksLogLevel Level { get; init; } = OksLogLevel.Info;
    public OksLogCategory Category { get; init; } = OksLogCategory.Request;

    public string Message { get; init; } = string.Empty;

    public string? Exception { get; init; }

    public string? CorrelationId { get; init; }
    public string? UserId { get; init; }
    public string? ClientIp { get; init; }
    public string? HttpMethod { get; init; }
    public string? Path { get; init; }

    public int? StatusCode { get; init; }

    public long? ElapsedMilliseconds { get; init; }

    /// <summary>
    /// Ek alanlar: RateLimit, Repository vs. tabloya giderken JSON parse edilebilir.
    /// Örneğin: { "RequestsPerMinute": 60, "CurrentCount": 75 }
    /// </summary>
    public string? ExtraDataJson { get; init; }

    public OksLogTarget Target { get; init; } = OksLogTarget.Database;
}