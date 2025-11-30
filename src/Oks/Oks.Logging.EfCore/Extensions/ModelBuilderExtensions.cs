using Microsoft.EntityFrameworkCore;
using Oks.Logging.EfCore.Configurations;

namespace Oks.Logging.EfCore.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Uygulamanın DbContext'ine OKS log tablolarını dahil eder.
    /// Bunu çağıran her uygulama migration aldığında OksLog* tabloları otomatik oluşur.
    /// </summary>
    public static ModelBuilder AddOksLogging(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OksLogRequestConfiguration());
        modelBuilder.ApplyConfiguration(new OksLogPerformanceConfiguration());
        modelBuilder.ApplyConfiguration(new OksLogRateLimitConfiguration());
        modelBuilder.ApplyConfiguration(new OksLogRepositoryConfiguration());
        modelBuilder.ApplyConfiguration(new OksLogExceptionConfiguration());
        modelBuilder.ApplyConfiguration(new OksLogCustomConfiguration());
        modelBuilder.ApplyConfiguration(new OksLogAuditConfiguration());

        return modelBuilder;
    }
}