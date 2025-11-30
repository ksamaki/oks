namespace Oks.Logging.Abstractions.Enums;

public enum OksLogCategory
{
    Request = 0,
    Performance = 1,
    RateLimit = 2,
    RepositoryRead = 3,
    RepositoryWrite = 4,
    Exception = 5,
    Custom = 6,
    Audit = 7
}