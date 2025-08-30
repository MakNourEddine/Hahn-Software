using Domain.Common;

namespace Domain.Appointments.Events
{
    public sealed class AppointmentBookedEvent(Guid appointmentId, Guid dentistId, Guid patientId, DateTimeOffset startUtc, int durationMinutes) : BaseEvent
    {
        public Guid AppointmentId { get; } = appointmentId;
        public Guid DentistId { get; } = dentistId;
        public Guid PatientId { get; } = patientId;
        public DateTimeOffset StartUtc { get; } = startUtc;
        public int DurationMinutes { get; } = durationMinutes;
    }
}
