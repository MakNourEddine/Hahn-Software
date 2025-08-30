using Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Appointments
{
    public static class CancelAppointment
    {
        public record Command(Guid AppointmentId, string Reason) : IRequest;


        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
        }


        public sealed class Handler(IApplicationDbContext db) : IRequestHandler<Command>
        {
            public async Task Handle(Command request, CancellationToken ct)
            {
                var appt = await db.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, ct)
                ?? throw new KeyNotFoundException("Appointment not found");
                appt.Cancel(request.Reason);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
