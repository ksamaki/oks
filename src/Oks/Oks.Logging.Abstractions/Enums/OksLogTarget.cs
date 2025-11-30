namespace Oks.Logging.Abstractions.Enums;

[Flags]
public enum OksLogTarget
{
    None = 0,
    Database = 1,
    File = 2,
    Both = Database | File
}