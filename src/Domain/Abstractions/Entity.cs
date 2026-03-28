using Connecvita.Domain.Abstractions;

namespace Connecvita.Domain.Common;

public abstract class Entity<T>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Entity(T id)
    {
        Id = id;
    }

    protected Entity()
    { }

    public T Id { get; init; }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}