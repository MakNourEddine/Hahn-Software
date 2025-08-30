using Application.Abstractions;
using Domain.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Appointments
{
    public static class ListAppointmentsByDentist
    {
        public record Query(Guid DentistId, DateOnly? Date) : IRequest<IReadOnlyList<Dto>>;
        public record Dto(Guid Id, DateTimeOffset StartUtc, int DurationMinutes, AppointmentStatus Status, Guid PatientId, Guid ServiceId);


        public sealed class Handler(IApplicationDbContext db) : IRequestHandler<Query, IReadOnlyList<Dto>>
        {
            public async Task<IReadOnlyList<Dto>> Handle(Query request, CancellationToken ct)
            {
                var q = db.Appointments.AsQueryable().Where(a => a.DentistId == request.DentistId);
                if (request.Date is { } day)
                {
                    var start = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                    var end = start.AddDays(1);
                    q = q.Where(a => a.StartUtc >= start && a.StartUtc < end);
                }


                return await q
                .OrderBy(a => a.StartUtc)
                .Select(a => new Dto(a.Id, a.StartUtc, a.DurationMinutes, a.Status, a.PatientId, a.ServiceId))
                .ToListAsync(ct);
            }
        }
    }
}
