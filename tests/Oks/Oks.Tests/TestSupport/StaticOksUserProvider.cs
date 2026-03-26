using Oks.Persistence.Abstractions;

namespace Oks.Tests.TestSupport;

public sealed class StaticOksUserProvider : IOksUserProvider
{
    private readonly string? _userIdentifier;

    public StaticOksUserProvider(string? userIdentifier)
    {
        _userIdentifier = userIdentifier;
    }

    public string? GetCurrentUserIdentifier() => _userIdentifier;
}
