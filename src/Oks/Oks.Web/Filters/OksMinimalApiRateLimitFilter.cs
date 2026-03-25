using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Extensions;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Shared.Results;
using Oks.Web.Abstractions.Attributes;
using Oks.Web.RateLimiting;

namespace Oks.Web.Filters;

public sealed class OksMinimalApiRateLimitFilter : IEndpointFilter
{
    private readonly IMemoryCache _cache;
    private readonly OksRateLimitOptions _options;
    private readonly IOksLogWriter _logWriter;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public OksMinimalApiRateLimitFilter(
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

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!_options.Enabled)
            return await next(context);

        var httpContext = context.HttpContext;
        bool skip = httpContext.GetEndpoint()?.Metadata.GetMetadata<OksSkipRateLimitAttribute>() is not null;
        int limit = httpContext.GetEndpoint()?.Metadata.GetMetadata<OksRateLimitAttribute>()?.RequestsPerMinute
            ?? _options.DefaultRequestsPerMinute;

        if (limit <= 0)
            return await next(context);

        var now = DateTime.UtcNow;
        var path = httpContext.Request.Path.ToString();
        var method = httpContext.Request.Method;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string cacheKey = $"oks_rl:{clientIp}:{method}:{path}";

        var state = _cache.Get<RateLimitState>(cacheKey);
        if (state is null || now - state.WindowStartUtc >= Window)
        {
            state = new RateLimitState { WindowStartUtc = now, Count = 1 };
        }
        else
        {
            state.Count++;
        }

        _cache.Set(cacheKey, state, now + Window);
        bool exceeded = state.Count > limit;

        if (!exceeded)
            return await next(context);

        await LogRateLimitAsync(httpContext, limit, state.Count, skip);

        if (skip)
            return await next(context);

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
            meta);

        return Results.Json(result, statusCode: StatusCodes.Status429TooManyRequests);
    }

    private async Task LogRateLimitAsync(HttpContext context, int limit, int currentCount, bool isSkip)
    {
        var entry = new OksLogEntry
        {
            Category = OksLogCategory.RateLimit,
            Level = isSkip ? OksLogLevel.Warning : OksLogLevel.Error,
            Message = isSkip
                ? "Rate limit aşıldı ancak OksSkipRateLimit ile bloklanmadı."
                : "Rate limit aşıldı ve istek bloklandı.",
            Path = context.Request.Path,
            HttpMethod = context.Request.Method,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = context.TraceIdentifier,
            StatusCode = isSkip ? null : StatusCodes.Status429TooManyRequests,
            CreatedAtUtc = DateTime.UtcNow,
            ExtraDataJson = JsonSerializer.Serialize(new
            {
                LimitPerMinute = limit,
                CurrentCount = currentCount,
                SkipEnforced = isSkip
            })
        };

        await _logWriter.SafeWriteAsync(entry);
    }
}
