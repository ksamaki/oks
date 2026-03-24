using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Web.Abstractions.Attributes;

namespace Oks.Web.Filters;

public class OksUnitOfWorkFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _unitOfWork;

    public OksUnitOfWorkFilter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var executedContext = await next();

        // 1) Exception varsa ve handle edilmediyse → commit etme
        if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
            return;

        // 2) [OksSkipTransaction] varsa → hiçbir şekilde SaveChanges çağırma
        if (ShouldSkipTransaction(executedContext))
            return;

        // 3) Buraya geldiysek SaveChanges çağrılır.
        // EfUnitOfWork içerisinde ChangeTracker.HasChanges() kontrolü var;
        // değişiklik yoksa metod hızlıca 0 döner.
        await _unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
    }

    private static bool ShouldSkipTransaction(ActionExecutedContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return false;

        bool skipOnMethod = cad.MethodInfo
            .GetCustomAttributes(typeof(OksSkipTransactionAttribute), inherit: true)
            .Any();

        bool skipOnController = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksSkipTransactionAttribute), inherit: true)
            .Any();

        return skipOnMethod || skipOnController;
    }

}
