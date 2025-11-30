using Oks.Logging.Abstractions.Models;

namespace Oks.Logging.Abstractions.Interfaces;

public interface IOksLogWriter
{
    Task WriteAsync(OksLogEntry entry, CancellationToken cancellationToken = default);
}