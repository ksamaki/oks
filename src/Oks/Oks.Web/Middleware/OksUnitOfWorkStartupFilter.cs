using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Oks.Web.Middleware;

public class OksUnitOfWorkStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<OksUnitOfWorkMiddleware>();
            next(app);
        };
    }
}
