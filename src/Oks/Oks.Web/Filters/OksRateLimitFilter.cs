using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Shared.Results;
using Oks.Web.Abstractions.Attributes;
using Oks.Web.RateLimiting;
using System.Text.Json;
using Oks.Logging.Abstractions.Extensions;

namespace Oks.Web.Filters;

public sealed class OksRateLimitFilter : IAsyncActionFilter
{
    private readonly IMemoryCache _cache;
    private readonly OksRateLimitOptions _options;
    private readonly IOksLogWriter _logWriter;

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public OksRateLimitFilter(
        IMemoryCache cache,
        IOptions<OksRateLimitOptions> options,
        IOksLogWriter logWriter)
    {
        _cache = cache;
        _options = options.Value;
        _logWriter = logWriter;
    }

    private sealed class RateLimitState
    {
        public DateTime WindowStartUtc { get; set; }
        public int Count { get; set; }
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!_options.Enabled)
        {
            await next();
            return;
        }

        // Attribute bilgileri
        bool skip = HasSkipRateLimit(context);
        int? customLimit = GetCustomRateLimit(context);

        int limit = customLimit ?? _options.DefaultRequestsPerMinute;
        if (limit <= 0)
        {
            await next();
            return;
        }

        var now = DateTime.UtcNow;

        var path = httpContext.Request.Path.ToString();
        var method = httpContext.Request.Method;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // IP + Path + Method bazlı key
        string cacheKey = $"oks_rl:{clientIp}:{method}:{path}";

        var state = _cache.Get<RateLimitState>(cacheKey);

        if (state is null || now - state.WindowStartUtc >= Window)
        {
            state = new RateLimitState
            {
                WindowStartUtc = now,
                Count = 1
            };
        }
        else
        {
            state.Count++;
        }

        _cache.Set(cacheKey, state, now + Window);

        bool exceeded = state.Count > limit;

        if (exceeded)
        {
            // Her durumda log at (skip olsa bile)
            await LogRateLimitAsync(httpContext, limit, state.Count, skip);

            if (!skip)
            {
                // Blokla → 429 + Result
                var meta = new Meta
                {
                    CorrelationId = httpContext.TraceIdentifier,
                    Extra = new Dictionary<string, string>
                    {
                        { "path", path },
                        { "method", method },
                        { "clientIp", clientIp },
                        { "limitPerMinute", limit.ToString() },
                        { "currentCount", state.Count.ToString() }
                    }
                };

                var result = Result.Fail(
                    "Çok fazla istek gönderdiniz. Lütfen daha sonra tekrar deneyin.",
                    ResultStatus.TooManyRequests,
                    meta
                );

                context.Result = new ObjectResult(result)
                {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };

                return;
            }
            // skip true ise: bloklama yok, devam edeceğiz.
        }

        await next();
    }

    private static bool HasSkipRateLimit(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return false;

        bool onMethod = cad.MethodInfo
            .GetCustomAttributes(typeof(OksSkipRateLimitAttribute), inherit: true)
            .Any();

        bool onController = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksSkipRateLimitAttribute), inherit: true)
            .Any();

        return onMethod || onController;
    }

    private static int? GetCustomRateLimit(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return null;

        var methodAttr = cad.MethodInfo
            .GetCustomAttributes(typeof(OksRateLimitAttribute), inherit: true)
            .Cast<OksRateLimitAttribute>()
            .FirstOrDefault();

        if (methodAttr is not null)
            return methodAttr.RequestsPerMinute;

        var classAttr = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksRateLimitAttribute), inherit: true)
            .Cast<OksRateLimitAttribute>()
            .FirstOrDefault();

        return classAttr?.RequestsPerMinute;
    }

    private async Task LogRateLimitAsync(HttpContext context, int limit, int currentCount, bool isSkip)
    {
        var httpRequest = context.Request;

        var extra = new
        {
            LimitPerMinute = limit,
            CurrentCount = currentCount,
            SkipEnforced = isSkip
        };

        var entry = new OksLogEntry
        {
            Category = OksLogCategory.RateLimit,
            Level = isSkip ? OksLogLevel.Warning : OksLogLevel.Error,
            Message = isSkip
                ? "Rate limit aşıldı ancak OksSkipRateLimit ile bloklanmadı."
                : "Rate limit aşıldı ve istek bloklandı.",
            Path = httpRequest.Path,
            HttpMethod = httpRequest.Method,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = context.TraceIdentifier,
            StatusCode = isSkip ? null : StatusCodes.Status429TooManyRequests,
            CreatedAtUtc = DateTime.UtcNow,
            ExtraDataJson = JsonSerializer.Serialize(extra)
        };

        await _logWriter.SafeWriteAsync(entry);
    }
}
