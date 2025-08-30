using Application.Abstractions;
using Domain.Appointments;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Appointments
{
    public static class RescheduleAppointment
    {
        public record Command(Guid AppointmentId, DateTimeOffset NewStartUtc) : IRequest;


        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.NewStartUtc)
                .Must(d => d > DateTimeOffset.UtcNow).WithMessage("Start must be in the future")
                .Must(d => d.Minute % 15 == 0 && d.Second == 0).WithMessage("Start must align to 15-minute grid");
            }
        }


        public sealed class Handler(IApplicationDbContext db) : IRequestHandler<Command>
        {
            private const int ClinicStartHourUtc = 9;
            private const int ClinicEndHourUtc = 17;


            public async Task Handle(Command request, CancellationToken ct)
            {
                var appt = await db.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
                ?? throw new KeyNotFoundException("Appointment not found");


                var newStart = request.NewStartUtc;
                if (newStart.Hour < ClinicStartHourUtc || newStart.Hour >= ClinicEndHourUtc)
                    throw new ValidationException("Outside clinic hours (09:00–17:00 UTC)");


                var newEnd = newStart.AddMinutes(appt.DurationMinutes);
                bool overlaps = await db.Appointments
                .AnyAsync(a => a.DentistId == appt.DentistId && a.Id != appt.Id && a.Status == AppointmentStatus.Scheduled &&
                a.StartUtc < newEnd && newStart < a.StartUtc.AddMinutes(a.DurationMinutes), ct);
                if (overlaps) throw new ValidationException("Conflicts with another appointment");


                appt.Reschedule(newStart);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
