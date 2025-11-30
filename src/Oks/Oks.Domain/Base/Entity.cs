namespace Oks.Domain.Base;

public abstract class Entity<TKey>
{
    public TKey Id { get; protected set; } = default!;

    protected Entity()
    {
    }

    protected Entity(TKey id)
    {
        Id = id;
    }
}
