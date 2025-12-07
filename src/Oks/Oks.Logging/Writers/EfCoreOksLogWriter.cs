using Microsoft.EntityFrameworkCore;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Logging.EfCore.Entities;
using System.Text.Json;

namespace Oks.Logging.Writers;

/// <summary>
/// OKS loglamanın EF Core implementasyonu.
/// OksLogEntry'yi category'ye göre ilgili tabloya mapler.
/// </summary>
public sealed class EfCoreOksLogWriter<TDbContext> : IOksLogWriter
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EfCoreOksLogWriter(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(OksLogEntry entry, CancellationToken cancellationToken = default)
    {
        switch (entry.Category)
        {
            case OksLogCategory.Request:
                await WriteRequestAsync(entry, cancellationToken);
                break;

            case OksLogCategory.Performance:
                await WritePerformanceAsync(entry, cancellationToken);
                break;

            case OksLogCategory.RateLimit:
                await WriteRateLimitAsync(entry, cancellationToken);
                break;

            case OksLogCategory.RepositoryRead:
            case OksLogCategory.RepositoryWrite:
                await WriteRepositoryAsync(entry, cancellationToken);
                break;

            case OksLogCategory.Exception:
                await WriteExceptionAsync(entry, cancellationToken);
                break;

            case OksLogCategory.Custom:
                await WriteCustomAsync(entry, cancellationToken);
                break;

            case OksLogCategory.Audit:
                await WriteAuditAsync(entry, cancellationToken);
                break;

            default:
                // Default: Request tablosuna düşür
                await WriteRequestAsync(entry, cancellationToken);
                break;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task WriteRequestAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogRequest
        {
            CreatedAtUtc = e.CreatedAtUtc,
            Level = e.Level,
            Message = e.Message,
            CorrelationId = e.CorrelationId,
            UserId = e.UserId,
            ClientIp = e.ClientIp,
            HttpMethod = e.HttpMethod,
            Path = e.Path,
            StatusCode = e.StatusCode,
            ElapsedMilliseconds = e.ElapsedMilliseconds
        };

        await _dbContext.Set<OksLogRequest>().AddAsync(log, ct);
    }

    private async Task WritePerformanceAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogPerformance
        {
            CreatedAtUtc = e.CreatedAtUtc,
            Level = e.Level,
            Message = e.Message,
            CorrelationId = e.CorrelationId,
            UserId = e.UserId,
            Path = e.Path,
            ElapsedMilliseconds = e.ElapsedMilliseconds ?? 0,
            ThresholdMilliseconds = Convert.ToInt64(
                (e.ExtraDataJson != null) ?
                System.Text.Json.JsonDocument.Parse(e.ExtraDataJson)
                    .RootElement.GetProperty("Threshold").GetInt32() : 0
            ),
            ExtraDataJson = e.ExtraDataJson
        };

        await _dbContext.Set<OksLogPerformance>().AddAsync(log, ct);
    }

    private async Task WriteRateLimitAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogRateLimit
        {
            CreatedAtUtc = e.CreatedAtUtc,
            Level = e.Level,
            Message = e.Message,
            CorrelationId = e.CorrelationId,
            UserId = e.UserId,
            ClientIp = e.ClientIp,
            Path = e.Path,
            HttpMethod = e.HttpMethod,
            StatusCode = e.StatusCode,
            ExtraDataJson = e.ExtraDataJson
        };

        await _dbContext.Set<OksLogRateLimit>().AddAsync(log, ct);
    }

    private async Task WriteRepositoryAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogRepository
        {
            CreatedAtUtc = e.CreatedAtUtc,
            Level = e.Level,
            Message = e.Message,
            CorrelationId = e.CorrelationId,
            UserId = e.UserId,
            EntityName = ExtractEntityName(e),
            OperationType = e.Category == OksLogCategory.RepositoryRead
                                ? "Read"
                                : "Write",
            ElapsedMilliseconds = e.ElapsedMilliseconds,
            ExtraDataJson = e.ExtraDataJson
        };

        await _dbContext.Set<OksLogRepository>().AddAsync(log, ct);
    }

    private async Task WriteExceptionAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogException
        {
            CreatedAtUtc = e.CreatedAtUtc,
            Level = e.Level,
            Message = e.Message,
            Exception = e.Exception,
            Path = e.Path,
            HttpMethod = e.HttpMethod,
            StatusCode = e.StatusCode?.ToString(),
            CorrelationId = e.CorrelationId,
            UserId = e.UserId,
            ClientIp = e.ClientIp
        };

        await _dbContext.Set<OksLogException>().AddAsync(log, ct);
    }

    private async Task WriteCustomAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogCustom
        {
            CreatedAtUtc = e.CreatedAtUtc,
            Level = e.Level,
            Message = e.Message,
            CorrelationId = e.CorrelationId,
            UserId = e.UserId,
            ClientIp = e.ClientIp,
            ExtraDataJson = e.ExtraDataJson
        };

        await _dbContext.Set<OksLogCustom>().AddAsync(log, ct);
    }

    private async Task WriteAuditAsync(OksLogEntry e, CancellationToken ct)
    {
        var log = new OksLogAudit
        {
            CreatedAtUtc = e.CreatedAtUtc,
            EntityName = ExtractEntityName(e),
            EntityId = ExtractEntityId(e),
            Operation = ExtractOperation(e),
            OldValuesJson = ExtractOldValues(e),
            NewValuesJson = ExtractNewValues(e),
            UserId = e.UserId,
            CorrelationId = e.CorrelationId
        };

        await _dbContext.Set<OksLogAudit>().AddAsync(log, ct);
    }

    private string ExtractEntityName(OksLogEntry e)
    {
        if (string.IsNullOrWhiteSpace(e.ExtraDataJson))
            return "UnknownEntity";

        try
        {
            using var doc = JsonDocument.Parse(e.ExtraDataJson);
            if (doc.RootElement.TryGetProperty("EntityName", out var prop))
            {
                return prop.GetString() ?? "UnknownEntity";
            }
        }
        catch { }

        return "UnknownEntity";
    }

    private string ExtractEntityId(OksLogEntry e)
    {
        if (string.IsNullOrWhiteSpace(e.ExtraDataJson))
            return "0";

        try
        {
            using var doc = JsonDocument.Parse(e.ExtraDataJson);
            if (doc.RootElement.TryGetProperty("EntityId", out var prop))
            {
                return prop.GetString() ?? "0";
            }
        }
        catch { }

        return "0";
    }

    private string ExtractOperation(OksLogEntry e)
    {
        if (string.IsNullOrWhiteSpace(e.ExtraDataJson))
            return e.Category == OksLogCategory.RepositoryRead ? "Read" :
                   e.Category == OksLogCategory.RepositoryWrite ? "Write" :
                   "Unknown";

        try
        {
            using var doc = JsonDocument.Parse(e.ExtraDataJson);
            if (doc.RootElement.TryGetProperty("Operation", out var prop))
            {
                return prop.GetString() ??
                       (e.Category == OksLogCategory.RepositoryRead ? "Read" :
                        e.Category == OksLogCategory.RepositoryWrite ? "Write" : "Unknown");
            }
        }
        catch { }

        return e.Category == OksLogCategory.RepositoryRead ? "Read" :
               e.Category == OksLogCategory.RepositoryWrite ? "Write" : "Unknown";
    }

    private string? ExtractOldValues(OksLogEntry e)
    {
        if (string.IsNullOrWhiteSpace(e.ExtraDataJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(e.ExtraDataJson);
            if (doc.RootElement.TryGetProperty("OldValues", out var prop))
            {
                return prop.ValueKind == JsonValueKind.Null
                    ? null
                    : prop.GetRawText(); // JSON string olarak saklıyoruz
            }
        }
        catch { }

        return null;
    }

    private string? ExtractNewValues(OksLogEntry e)
    {
        if (string.IsNullOrWhiteSpace(e.ExtraDataJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(e.ExtraDataJson);
            if (doc.RootElement.TryGetProperty("NewValues", out var prop))
            {
                return prop.ValueKind == JsonValueKind.Null
                    ? null
                    : prop.GetRawText();
            }
        }
        catch { }

        return null;
    }
}