using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;

namespace Oks.Web.Middleware;

public sealed class OksRequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<OksRequestLoggingMiddleware> _logger;
    private readonly IOksLogWriter _logWriter;

    public OksRequestLoggingMiddleware(
        ILogger<OksRequestLoggingMiddleware> logger,
        IOksLogWriter logWriter)
    {
        _logger = logger;
        _logWriter = logWriter;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();

        await next(context);

        sw.Stop();

        var request = context.Request;
        var response = context.Response;

        var statusCode = response.StatusCode;

        var level = statusCode switch
        {
            >= 500 => OksLogLevel.Error,
            >= 400 => OksLogLevel.Warning,
            _ => OksLogLevel.Info
        };

        var entry = new OksLogEntry
        {
            Category = OksLogCategory.Request,
            Level = level,
            Message = $"HTTP {request.Method} {request.Path} -> {statusCode} ({sw.ElapsedMilliseconds} ms)",

            Path = request.Path,
            HttpMethod = request.Method,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            StatusCode = statusCode,
            CreatedAtUtc = DateTime.UtcNow,
            ElapsedMilliseconds = sw.ElapsedMilliseconds,
            CorrelationId = context.TraceIdentifier,
            UserId = context.User?.Identity?.IsAuthenticated == true
                        ? context.User.Identity!.Name
                        : null
        };

        // Eğer log yazarken hata olursa uygulamayı düşürmemek için try/catch ile sarıyoruz
        try
        {
            await _logWriter.WriteAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OksRequestLoggingMiddleware: log yazılırken hata oluştu.");
        }
    }
}