using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Oks.Domain.Base;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Extensions;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore.Options;
using System.Text.Json;

namespace Oks.Persistence.EfCore;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    private readonly IOksLogWriter? _logWriter;
    private readonly OksAuditLoggingOptions _auditOptions;

    public EfUnitOfWork(
        DbContext dbContext,
        IOksLogWriter? logWriter = null,
        IOptions<OksAuditLoggingOptions>? auditOptions = null)
    {
        _dbContext = dbContext;
        _logWriter = logWriter;
        _auditOptions = auditOptions?.Value ?? new OksAuditLoggingOptions
        {
            Enabled = false
        };
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_dbContext.ChangeTracker.HasChanges())
            return 0;

        // Audit kapalıysa veya logWriter yoksa: sadece normal SaveChanges
        if (!_auditOptions.Enabled || _logWriter is null)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var auditEntries = BuildAuditEntries();

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            await WriteAuditLogsAsync(auditEntries, cancellationToken);
        }

        return result;
    }

    // ----------------- YARDIMCI ISLER -----------------

    private sealed class AuditEntry
    {
        public string EntityName { get; init; } = default!;
        public string EntityId { get; init; } = "0";
        public string Operation { get; init; } = default!; // Insert / Update / Delete

        public Dictionary<string, object?>? OldValues { get; init; }
        public Dictionary<string, object?>? NewValues { get; init; }
    }

    private List<AuditEntry> BuildAuditEntries()
    {
        var result = new List<AuditEntry>();

        var entries = _dbContext.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is not null &&
                IsOksEntity(e.Entity.GetType()) &&
                (e.State == EntityState.Added ||
                 e.State == EntityState.Modified ||
                 e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            var entityType = entry.Entity!.GetType();
            var entityName = entityType.Name;

            // Entity<TKey>.Id property reflection ile bul
            var idProp = entityType.GetProperty(nameof(Entity<object>.Id));
            var idValue = idProp?.GetValue(entry.Entity)?.ToString() ?? "0";

            string operation = entry.State switch
            {
                EntityState.Added => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            Dictionary<string, object?>? oldValues = null;
            Dictionary<string, object?>? newValues = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues = entry.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    break;

                case EntityState.Modified:
                    oldValues = new Dictionary<string, object?>();
                    newValues = new Dictionary<string, object?>();

                    foreach (var prop in entry.Properties.Where(p => p.IsModified))
                    {
                        var name = prop.Metadata.Name;
                        oldValues[name] = prop.OriginalValue;
                        newValues[name] = prop.CurrentValue;
                    }

                    // Hiç değişen property yoksa loglama yapmaya gerek yok
                    if (oldValues.Count == 0 && newValues.Count == 0)
                        continue;
                    break;

                case EntityState.Deleted:
                    oldValues = entry.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    break;
            }

            var auditEntry = new AuditEntry
            {
                EntityName = entityName,
                EntityId = idValue,
                Operation = operation,
                OldValues = oldValues,
                NewValues = newValues
            };

            result.Add(auditEntry);
        }

        return result;
    }

    private async Task WriteAuditLogsAsync(
        List<AuditEntry> auditEntries,
        CancellationToken cancellationToken)
    {
        foreach (var audit in auditEntries)
        {
            var extra = new
            {
                audit.EntityName,
                audit.EntityId,
                audit.Operation,
                audit.OldValues,
                audit.NewValues
            };

            var entry = new OksLogEntry
            {
                Category = OksLogCategory.Audit,
                Level = OksLogLevel.Info,
                Message = $"{audit.Operation} on {audit.EntityName} ({audit.EntityId})",
                CreatedAtUtc = DateTime.UtcNow,
                ExtraDataJson = JsonSerializer.Serialize(extra)
                // İleride CorrelationId, UserId, ClientIp gibi alanları doldurmak için
                // current user / correlation abstraction ekleyebiliriz.
            };

            await _logWriter.SafeWriteAsync(entry, cancellationToken);
        }
    }

    /// <summary>
    /// Verilen tip Oks.Entity<TKey> hiyerarşisinden geliyor mu?
    /// </summary>
    private static bool IsOksEntity(Type type)
    {
        while (type is not null && type != typeof(object))
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return true;
            }

            type = type.BaseType!;
        }

        return false;
    }
}