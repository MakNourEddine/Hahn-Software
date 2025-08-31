namespace Domain.Common
{
    public abstract class BaseEntity
    {
        private readonly List<BaseEvent> _domainEvents = [];
        public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();


        protected void AddDomainEvent(BaseEvent @event) => _domainEvents.Add(@event);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
