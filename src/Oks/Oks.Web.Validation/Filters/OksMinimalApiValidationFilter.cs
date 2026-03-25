using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Oks.Shared.Results;
using Oks.Web.Abstractions.Attributes;
using Oks.Web.Validation.Internal;

namespace Oks.Web.Validation.Filters;

public sealed class OksMinimalApiValidationFilter : IEndpointFilter
{
    private readonly IServiceProvider _serviceProvider;

    public OksMinimalApiValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (ShouldSkipValidation(context.HttpContext))
            return await next(context);

        var failures = new List<ValidationFailure>();

        foreach (var argument in context.Arguments)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
                failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
            return await next(context);

        var payload = DataResult<Dictionary<string, string[]>>.Fail(
            data: OksValidationErrorBuilder.BuildDictionary(failures),
            message: "Doğrulama hatası oluştu.",
            status: ResultStatus.BadRequest);

        return Results.BadRequest(payload);
    }

    private static bool ShouldSkipValidation(HttpContext httpContext)
        => httpContext.GetEndpoint()?.Metadata.GetMetadata<OksSkipValidationAttribute>() is not null;
}
