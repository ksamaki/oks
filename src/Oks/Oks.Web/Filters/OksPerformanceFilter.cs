using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Shared.Results;
using Oks.Web.Abstractions.Attributes;
using Oks.Web.Performance;

namespace Oks.Web.Filters;

public sealed class OksPerformanceFilter : IAsyncActionFilter
{
    private readonly OksPerformanceOptions _options;
    private readonly IOksLogWriter _logWriter;

    public OksPerformanceFilter(
        IOptions<OksPerformanceOptions> options,
        IOksLogWriter logWriter)
    {
        _options = options.Value;
        _logWriter = logWriter;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!_options.Enabled)
        {
            await next();
            return;
        }

        var httpContext = context.HttpContext;
        var path = httpContext.Request.Path.ToString();
        var method = httpContext.Request.Method;
        var traceId = httpContext.TraceIdentifier;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

        bool skip = HasSkipPerformance(context);
        int threshold = GetCustomThreshold(context) ?? _options.DefaultThresholdMs;

        if (threshold <= 0)
        {
            await next();
            return;
        }

        var sw = Stopwatch.StartNew();
        var executedContext = await next();
        sw.Stop();

        long elapsedMs = sw.ElapsedMilliseconds;

        // Threshold altında ise: İleride loglayabiliriz, şimdilik sessiz.
        if (elapsedMs <= threshold)
            return;

        // Her durumda performance log at (skip olsa da).
        var extra = new
        {
            Threshold = threshold,
            Elapsed = elapsedMs,
            SkipEnforced = skip
        };

        var entry = new OksLogEntry
        {
            Category = OksLogCategory.Performance,
            Level = (!skip && _options.ThrowOnSlowRequest) ? OksLogLevel.Error : OksLogLevel.Warning,
            Message = $"Yavaş istek tespit edildi. Elapsed={elapsedMs}ms, Threshold={threshold}ms.",
            Path = path,
            HttpMethod = method,
            ClientIp = clientIp,
            CorrelationId = traceId,
            StatusCode = executedContext.HttpContext.Response?.StatusCode,
            CreatedAtUtc = DateTime.UtcNow,
            ElapsedMilliseconds = elapsedMs,
            ExtraDataJson = JsonSerializer.Serialize(extra)
        };

        await _logWriter.WriteAsync(entry);

        // Skip değilse ve ThrowOnSlowRequest true ise exception fırlat
        if (!skip && _options.ThrowOnSlowRequest)
        {
            throw new OksPerformanceException(
                $"İstek süresi threshold'u aştı. Elapsed={elapsedMs}ms, Threshold={threshold}ms.",
                elapsedMs,
                threshold
            );
        }

        // skip == true ise sadece loglayıp devam ediyoruz
    }

    private static bool HasSkipPerformance(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return false;

        bool onMethod = cad.MethodInfo
            .GetCustomAttributes(typeof(OksSkipPerformanceAttribute), inherit: true)
            .Any();

        bool onController = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksSkipPerformanceAttribute), inherit: true)
            .Any();

        return onMethod || onController;
    }

    private static int? GetCustomThreshold(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return null;

        var methodAttr = cad.MethodInfo
            .GetCustomAttributes(typeof(OksPerformanceAttribute), inherit: true)
            .Cast<OksPerformanceAttribute>()
            .FirstOrDefault();

        if (methodAttr is not null)
            return methodAttr.ThresholdMs;

        var classAttr = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksPerformanceAttribute), inherit: true)
            .Cast<OksPerformanceAttribute>()
            .FirstOrDefault();

        return classAttr?.ThresholdMs;
    }
}