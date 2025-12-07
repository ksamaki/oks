using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;

namespace Oks.Logging.Abstractions.Extensions;

public static class OksLogWriterExtensions
{
    public static Task SafeWriteAsync(
        this IOksLogWriter? writer,
        OksLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        if (writer is null)
            return Task.CompletedTask;

        return writer.WriteAsync(entry, cancellationToken);
    }
}