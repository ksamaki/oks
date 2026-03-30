using Oks.Caching.Abstractions;
using Oks.Domain.Base;

namespace Oks.Tests.TestSupport;

[OksEntityCache(TtlSeconds = 120, Tags = ["test-user"])]
public class TestUser : AuditedEntity<int>
{
    public string Name { get; set; } = default!;
}
