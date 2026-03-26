namespace Oks.Persistence.Abstractions;

public interface IOksUserProvider
{
    string? GetCurrentUserIdentifier();
}
