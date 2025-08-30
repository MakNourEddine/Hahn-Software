using Domain.Appointments.Events;
using Domain.Common;
using Domain.Dentists;
using Domain.Patients;
using Domain.Services;

namespace Domain.Appointments
{
    public class Appointment : BaseEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid DentistId { get; private set; }
        public Guid PatientId { get; private set; }
        public Guid ServiceId { get; private set; }
        public DateTimeOffset StartUtc { get; private set; }
        public int DurationMinutes { get; private set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;

        public Dentist Dentist { get; private set; } = default!;
        public Patient Patient { get; private set; } = default!;
        public Service Service { get; private set; } = default!;

        public DateTimeOffset EndUtc => StartUtc.AddMinutes(DurationMinutes);


        private Appointment() { }


        public Appointment(Dentist dentist, Patient patient, Service service, DateTimeOffset startUtc)
        {
            Dentist = dentist ?? throw new ArgumentNullException(nameof(dentist));
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));
            Service = service ?? throw new ArgumentNullException(nameof(service));

            DentistId = dentist.Id;
            PatientId = patient.Id;
            ServiceId = service.Id;

            if (startUtc <= DateTimeOffset.UtcNow) throw new ArgumentException("Start must be in the future");
            if (service.DurationMinutes % 15 != 0) throw new ArgumentException("Duration must align to 15-minute grid");

            StartUtc = startUtc;
            DurationMinutes = service.DurationMinutes;

            AddDomainEvent(new AppointmentBookedEvent(Id, DentistId, PatientId, StartUtc, DurationMinutes));
        }


        public void Cancel(string reason)
        {
            if (Status == AppointmentStatus.Cancelled) return;
            Status = AppointmentStatus.Cancelled;
            AddDomainEvent(new AppointmentCancelledEvent(Id, reason));
        }


        public void Reschedule(DateTimeOffset newStartUtc)
        {
            if (Status == AppointmentStatus.Cancelled) throw new InvalidOperationException("Cannot reschedule cancelled appointment");
            if (newStartUtc <= DateTimeOffset.UtcNow) throw new ArgumentException("Start must be in the future");
            StartUtc = newStartUtc;
            AddDomainEvent(new AppointmentRescheduledEvent(Id, newStartUtc));
        }
    }
}
