namespace Oks.Persistence.EfCore;

public sealed class WriteTracker
{
    public bool HasWrite { get; private set; }

    public void MarkWrite()
    {
        HasWrite = true;
    }
}
