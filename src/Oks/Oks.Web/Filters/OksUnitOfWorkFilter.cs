using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Oks.Persistence.Abstractions.Repositories;
using Oks.Persistence.EfCore;
using Oks.Web.Abstractions.Attributes;

namespace Oks.Web.Filters;

public class OksUnitOfWorkFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly WriteTracker _writeTracker;

    public OksUnitOfWorkFilter(IUnitOfWork unitOfWork, WriteTracker writeTracker)
    {
        _unitOfWork = unitOfWork;
        _writeTracker = writeTracker;
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

        // 3) Yazma yoksa ve [OksTransactional] ile zorlanmamışsa → SaveChanges çağırmaya gerek yok
        if (!_writeTracker.HasWrite && !HasOksTransactional(executedContext))
            return;

        // 4) Buraya geldiysek → ya yazma oldu, ya da OksTransactional ile zorunlu kılındı
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

    private static bool HasOksTransactional(ActionExecutedContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return false;

        bool onMethod = cad.MethodInfo
            .GetCustomAttributes(typeof(OksTransactionalAttribute), inherit: true)
            .Any();

        bool onController = cad.ControllerTypeInfo
            .GetCustomAttributes(typeof(OksTransactionalAttribute), inherit: true)
            .Any();

        return onMethod || onController;
    }
}