using Domain.Common;

namespace Domain.Appointments.Events
{
    public sealed class AppointmentRescheduledEvent(Guid appointmentId, DateTimeOffset newStartUtc) : BaseEvent
    {
        public Guid AppointmentId { get; } = appointmentId;
        public DateTimeOffset NewStartUtc { get; } = newStartUtc;
    }
}
