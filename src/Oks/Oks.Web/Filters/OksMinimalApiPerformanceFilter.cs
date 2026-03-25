using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Extensions;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Web.Abstractions.Attributes;
using Oks.Web.Performance;

namespace Oks.Web.Filters;

public sealed class OksMinimalApiPerformanceFilter : IEndpointFilter
{
    private readonly OksPerformanceOptions _options;
    private readonly IOksLogWriter _logWriter;

    public OksMinimalApiPerformanceFilter(
        IOptions<OksPerformanceOptions> options,
        IOksLogWriter logWriter)
    {
        _options = options.Value;
        _logWriter = logWriter;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!_options.Enabled)
            return await next(context);

        var httpContext = context.HttpContext;
        bool skip = httpContext.GetEndpoint()?.Metadata.GetMetadata<OksSkipPerformanceAttribute>() is not null;
        int threshold = httpContext.GetEndpoint()?.Metadata.GetMetadata<OksPerformanceAttribute>()?.ThresholdMs
            ?? _options.DefaultThresholdMs;

        if (threshold <= 0)
            return await next(context);

        var sw = Stopwatch.StartNew();
        var response = await next(context);
        sw.Stop();

        long elapsedMs = sw.ElapsedMilliseconds;
        if (elapsedMs <= threshold)
            return response;

        var entry = new OksLogEntry
        {
            Category = OksLogCategory.Performance,
            Level = (!skip && _options.ThrowOnSlowRequest) ? OksLogLevel.Error : OksLogLevel.Warning,
            Message = $"Yavaş istek tespit edildi. Elapsed={elapsedMs}ms, Threshold={threshold}ms.",
            Path = httpContext.Request.Path,
            HttpMethod = httpContext.Request.Method,
            ClientIp = httpContext.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = httpContext.TraceIdentifier,
            StatusCode = httpContext.Response.StatusCode,
            CreatedAtUtc = DateTime.UtcNow,
            ElapsedMilliseconds = elapsedMs,
            ExtraDataJson = JsonSerializer.Serialize(new { Threshold = threshold, Elapsed = elapsedMs, SkipEnforced = skip })
        };

        await _logWriter.SafeWriteAsync(entry);

        if (!skip && _options.ThrowOnSlowRequest)
        {
            throw new OksPerformanceException(
                $"İstek süresi threshold'u aştı. Elapsed={elapsedMs}ms, Threshold={threshold}ms.",
                elapsedMs,
                threshold);
        }

        return response;
    }
}
