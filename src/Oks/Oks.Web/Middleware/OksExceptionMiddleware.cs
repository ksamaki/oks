using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Oks.Logging.Abstractions.Enums;
using Oks.Logging.Abstractions.Interfaces;
using Oks.Logging.Abstractions.Models;
using Oks.Shared.Results;
using Oks.Logging.Abstractions.Extensions;

namespace Oks.Web.Middleware;

public sealed class OksExceptionMiddleware : IMiddleware
{
    private readonly ILogger<OksExceptionMiddleware> _logger;
    private readonly IOksLogWriter _logWriter;

    public OksExceptionMiddleware(
        ILogger<OksExceptionMiddleware> logger,
        IOksLogWriter logWriter)
    {
        _logger = logger;
        _logWriter = logWriter;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // 1) Normal console / file log (framework default LogError)
            _logger.LogError(ex, "Unhandled exception occurred.");

            // 2) SQLogException tablosuna log yaz
            await _logWriter.SafeWriteAsync(new OksLogEntry
            {
                Category = OksLogCategory.Exception,
                Level = OksLogLevel.Error,
                Message = ex.Message,
                Exception = ex.ToString(),
                Path = context.Request.Path,
                HttpMethod = context.Request.Method,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                CorrelationId = context.TraceIdentifier,
                StatusCode = StatusCodes.Status500InternalServerError,
                CreatedAtUtc = DateTime.UtcNow,
                UserId = context.User?.Identity?.IsAuthenticated == true
                    ? context.User.Identity.Name
                    : null
            });

            // 3) Kullanıcıya OKS Result formatında düzgün cevap dön
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentType = "application/json";

            var result = Result.Fail(
                message: "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                status: ResultStatus.Error,
                meta: new Meta
                {
                    CorrelationId = context.TraceIdentifier,
                    Extra = new Dictionary<string, string>
                    {
                        { "path", context.Request.Path },
                        { "method", context.Request.Method }
                    }
                }
            );


            await response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}