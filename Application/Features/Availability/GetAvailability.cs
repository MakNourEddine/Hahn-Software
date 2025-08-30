using Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Availability
{
    public static class GetAvailability
    {
        public record Query(Guid DentistId, DateOnly Date) : IRequest<IReadOnlyList<Slot>>;
        public record Slot(DateTimeOffset StartUtc, DateTimeOffset EndUtc);


        public sealed class Handler(IApplicationDbContext db) : IRequestHandler<Query, IReadOnlyList<Slot>>
        {
            private const int ClinicStartHourUtc = 9;
            private const int ClinicEndHourUtc = 17;


            public async Task<IReadOnlyList<Slot>> Handle(Query request, CancellationToken ct)
            {
                var dayStart = new DateTimeOffset(request.Date.ToDateTime(new TimeOnly(ClinicStartHourUtc, 0)), TimeSpan.Zero);
                var dayEnd = new DateTimeOffset(request.Date.ToDateTime(new TimeOnly(ClinicEndHourUtc, 0)), TimeSpan.Zero);


                var appts = await db.Appointments
                .Where(a => a.DentistId == request.DentistId && a.Status == Domain.Appointments.AppointmentStatus.Scheduled &&
                a.StartUtc >= dayStart && a.StartUtc < dayEnd)
                .Select(a => new { a.StartUtc, End = a.StartUtc.AddMinutes(a.DurationMinutes) })
                .OrderBy(a => a.StartUtc)
                .ToListAsync(ct);


                var slots = new List<Slot>();
                DateTimeOffset cursor = dayStart;
                foreach (var a in appts)
                {
                    if (cursor < a.StartUtc)
                    {
                        AddGridSlots(slots, cursor, a.StartUtc);
                    }
                    cursor = a.End > cursor ? a.End : cursor;
                }
                if (cursor < dayEnd) AddGridSlots(slots, cursor, dayEnd);
                return slots;
            }


            private static void AddGridSlots(List<Slot> acc, DateTimeOffset start, DateTimeOffset end)
            {
                var cur = start;
                while (cur < end)
                {
                    var next = cur.AddMinutes(15);
                    if (next <= end) acc.Add(new Slot(cur, next));
                    cur = next;
                }
            }
        }
    }
}
