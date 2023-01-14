using CQRS.Core.Domain;

namespace CQRS.Core.Handlers
{
    public interface IEventSourcingHandler<T>
    {
        Task SaveAsync(AggregateRoot aggregate);
        Task<T> GetByIdAsync(Guid aggregateId); // T is the concrete aggregate implemetation, in our case the post aggregate
        Task RepublishEventsAsync();
    }
}