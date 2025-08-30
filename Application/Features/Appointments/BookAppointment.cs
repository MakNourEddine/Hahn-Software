using Application.Abstractions;
using Domain.Appointments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Appointments
{
    public static class BookAppointment
    {
        public record Command(Guid DentistId, Guid PatientId, Guid ServiceId, DateTimeOffset StartUtc) : IRequest<Guid>;


        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.DentistId).NotEmpty();
                RuleFor(x => x.PatientId).NotEmpty();
                RuleFor(x => x.ServiceId).NotEmpty();
                RuleFor(x => x.StartUtc)
                .Must(d => d > DateTimeOffset.UtcNow).WithMessage("Start must be in the future")
                .Must(d => d.Minute % 15 == 0 && d.Second == 0).WithMessage("Start must align to 15-minute grid");
            }
        }


        public sealed class Handler(IApplicationDbContext db) : IRequestHandler<Command, Guid>
        {
            private const int ClinicStartHourUtc = 9; // 09:00–17:00 UTC demo hours
            private const int ClinicEndHourUtc = 17;


            public async Task<Guid> Handle(Command request, CancellationToken ct)
            {
                var dentist = await db.Dentists.FirstOrDefaultAsync(d => d.Id == request.DentistId, ct)
                ?? throw new KeyNotFoundException("Dentist not found");
                var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId, ct)
                ?? throw new KeyNotFoundException("Patient not found");
                var service = await db.Services.FirstOrDefaultAsync(s => s.Id == request.ServiceId, ct)
                ?? throw new KeyNotFoundException("Service not found");


                var start = request.StartUtc;
                if (start.Hour < ClinicStartHourUtc || start.Hour >= ClinicEndHourUtc)
                    throw new ValidationException("Outside clinic hours (09:00–17:00 UTC)");


                var duration = TimeSpan.FromMinutes(service.DurationMinutes);
                var end = start.Add(duration);


                // Overlap check — Scheduled only
                bool overlaps = await db.Appointments
                .AnyAsync(a => a.DentistId == dentist.Id && a.Status == AppointmentStatus.Scheduled &&
                a.StartUtc < end && start < a.StartUtc.AddMinutes(a.DurationMinutes), ct);
                if (overlaps) throw new ValidationException("Dentist already has an appointment in that time window");


                var appt = new Appointment(dentist, patient, service, start);
                db.Appointments.Add(appt);
                await db.SaveChangesAsync(ct);
                return appt.Id;
            }
        }
    }
}
