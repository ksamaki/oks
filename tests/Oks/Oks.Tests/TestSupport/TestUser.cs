using Oks.Caching.Abstractions;
using Oks.Domain.Base;

namespace Oks.Tests.TestSupport;

[Cacheable(DurationSeconds = 120, Tags = ["test-user"])]
public class TestUser : AuditedEntity<int>
{
    public string Name { get; set; } = default!;
}
