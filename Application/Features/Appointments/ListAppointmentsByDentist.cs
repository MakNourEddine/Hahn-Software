using Application.Abstractions;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Appointments
{
    public static class ListAppointmentsByDentist
    {
        public record Query(Guid DentistId, DateOnly? Date)
            : IRequest<IReadOnlyList<AppointmentListItemDto>>;

        public sealed class Handler(IApplicationDbContext db)
            : IRequestHandler<Query, IReadOnlyList<AppointmentListItemDto>>
        {
            public async Task<IReadOnlyList<AppointmentListItemDto>> Handle(Query request, CancellationToken ct)
            {
                var q = db.Appointments.Where(a => a.DentistId == request.DentistId);

                if (request.Date is { } day)
                {
                    var start = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                    var end = start.AddDays(1);
                    q = q.Where(a => a.StartUtc >= start && a.StartUtc < end);
                }

                return await q
                    .OrderBy(a => a.StartUtc)
                    .Select(a => new AppointmentListItemDto(
                        a.Id,
                        a.StartUtc,
                        a.DurationMinutes,
                        a.Status,
                        a.PatientId,
                        a.Patient.FullName,
                        a.ServiceId,
                        a.Service.Name
                    ))
                    .ToListAsync(ct);
            }
        }
    }
}
