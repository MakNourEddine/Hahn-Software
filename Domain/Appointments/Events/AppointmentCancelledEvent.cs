using Domain.Common;

namespace Domain.Appointments.Events
{
    public sealed class AppointmentCancelledEvent(Guid appointmentId, string reason) : BaseEvent
    {
        public Guid AppointmentId { get; } = appointmentId;
        public string Reason { get; } = reason;
    }
}
