using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Oks.Shared.Results;
using Oks.Web.Abstractions.Attributes;
using Oks.Web.Validation.Internal;

namespace Oks.Web.Validation.Behaviors;

public sealed class OksValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public OksValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (ShouldSkipValidation())
            return await next();

        var validators = _validators as IValidator<TRequest>[] ?? _validators.ToArray();
        if (validators.Length == 0)
            return await next();

        var validationContext = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
            return await next();

        if (TryCreateFailureResponse(failures, out var failureResponse))
            return failureResponse!;

        throw new ValidationException(failures);
    }

    private static bool ShouldSkipValidation()
        => typeof(TRequest).GetCustomAttributes(typeof(OksSkipValidationAttribute), inherit: true).Any();

    private static bool TryCreateFailureResponse(IReadOnlyCollection<ValidationFailure> failures, out TResponse? response)
    {
        var errorDict = OksValidationErrorBuilder.BuildDictionary(failures);
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result) || responseType.IsSubclassOf(typeof(Result)))
        {
            var result = Result.Fail("Doğrulama hatası oluştu.", ResultStatus.BadRequest);
            if (result is TResponse casted)
            {
                response = casted;
                return true;
            }
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(DataResult<>))
        {
            var failMethod = responseType.GetMethod(
                "Fail",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { responseType.GetGenericArguments()[0], typeof(string), typeof(ResultStatus), typeof(Meta) },
                modifiers: null);

            if (failMethod is not null)
            {
                var failureData = responseType.GetGenericArguments()[0] == typeof(Dictionary<string, string[]>)
                    ? errorDict
                    : null;

                var invoked = failMethod.Invoke(
                    null,
                    new object?[] { failureData, "Doğrulama hatası oluştu.", ResultStatus.BadRequest, null });
                if (invoked is TResponse casted)
                {
                    response = casted;
                    return true;
                }
            }
        }

        response = default;
        return false;
    }
}
