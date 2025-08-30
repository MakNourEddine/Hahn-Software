using MediatR;

namespace Domain.Common
{
    public abstract class BaseEvent : INotification
    {
        public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    }
}
