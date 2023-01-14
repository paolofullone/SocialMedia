using CQRS.Core.Events;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        protected Guid _id; // será acessível somente as classes que extenderem a classe abstrata AggregateRoot
        private readonly List<BaseEvent> _changes = new();

        // Guid só retorna o _id, não entendi muito bem o "auto property" mas no vídeo fala que alternativamente poderia usar um "auto property" com public Guid Id { get; private set; }
        public Guid Id
        {
            get { return _id; }
        }

        public int Version { get; set; } = -1; // "This aggregate or the concrete aggregate that extends this class has not been persisted yet"

        public IEnumerable<BaseEvent> GetUncommittedChanges()
        {
            return _changes; // retorna a lista de eventos que ocorreram no aggregate
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear(); // clear the list of evetns, that needs to be done after the events are persisted (altered the state of the aggregate)
        }

        private void ApplyChange(BaseEvent @event, bool isNew)
        {
            var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() }); // get the method that will apply the event

            if (method is null)
            {
                throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the aggregate for {@event.GetType().Name}!");
            }

            method.Invoke(this, new object[] { @event }); // invoke the method that will apply the event

            if (isNew)
            {
                _changes.Add(@event); // add the event to the list of events that occurred in the aggregate, if the event is not new (already persisted) it will not be added to the list
            }
        }

        protected void RaiseEvent(BaseEvent @event)
        {
            ApplyChange(@event, true); // if we raise an event it is a new event, so we pass true to the ApplyChange method
        }

        public void ReplayEvents(IEnumerable<BaseEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyChange(@event, false); // if we replay events they are not new, so we pass false to the ApplyChange method
            }
        }
    }
}