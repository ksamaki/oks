using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Oks.Shared.Results;
using Oks.Web.Abstractions.Attributes;

namespace Oks.Web.Validation.Filters;

public class OksValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public OksValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (ShouldSkipValidation(context))
        {
            await next();
            return;
        }

        var failures = new List<ValidationFailure>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            var validator = _serviceProvider.GetService(validatorType) as IValidator;
            if (validator is null)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count > 0)
        {
            var errorDict = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            var result = DataResult<Dictionary<string, string[]>>.Fail(
                data: errorDict,
                message: "Doğrulama hatası oluştu.",
                status: ResultStatus.BadRequest);

            context.Result = new BadRequestObjectResult(result);
            return;
        }

        await next();
    }

    private static bool ShouldSkipValidation(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return false;

        // Method seviyesinde [OksSkipValidation] var mı?
        bool skipOnMethod = cad.MethodInfo
            .GetCustomAttributes(typeof(OksSkipValidationAttribute), inherit: true)
            .Any();

        // Controller seviyesinde [OksSkipValidation] var mı?
        bool skipOnController = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksSkipValidationAttribute), inherit: true)
            .Any();

        return skipOnMethod || skipOnController;
    }

    private static bool HasOksValidation(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return false;

        bool onMethod = cad.MethodInfo
            .GetCustomAttributes(typeof(OksValidationAttribute), inherit: true)
            .Any();

        bool onController = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksValidationAttribute), inherit: true)
            .Any();

        return onMethod || onController;
    }


}