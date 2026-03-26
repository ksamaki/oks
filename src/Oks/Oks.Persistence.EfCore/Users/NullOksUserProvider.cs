using Oks.Persistence.Abstractions;

namespace Oks.Persistence.EfCore.Users;

public sealed class NullOksUserProvider : IOksUserProvider
{
    public static readonly NullOksUserProvider Instance = new();

    private NullOksUserProvider()
    {
    }

    public string? GetCurrentUserIdentifier() => null;
}
