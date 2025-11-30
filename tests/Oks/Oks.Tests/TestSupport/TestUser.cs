using Oks.Domain.Base;

namespace Oks.Tests.TestSupport;

public class TestUser : AuditedEntity<int>
{
    public string Name { get; set; } = default!;
}