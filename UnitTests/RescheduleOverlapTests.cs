using Application.Features.Appointments;
using Domain.Appointments;
using Domain.Dentists;
using Domain.Patients;
using Domain.Services;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace UnitTests
{
    public class RescheduleOverlapTests
    {
        private static ApplicationDbContext Db()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new ApplicationDbContext(options, new NoopPublisher());
        }


        [Fact]
        public async Task Prevents_overlap_on_reschedule()
        {
            await using var db = Db();
            var dentist = new Dentist("Dr."); db.Dentists.Add(dentist);
            var patient = new Patient("John", "john@x.com"); db.Patients.Add(patient);
            var service = new Service("Clean", 30); db.Services.Add(service);
            await db.SaveChangesAsync();


            var start1 = DateTimeOffset.UtcNow.AddHours(1);
            var start2 = DateTimeOffset.UtcNow.AddHours(1).AddMinutes(15);
            db.Appointments.Add(new Appointment(dentist, patient, service, start1));
            var a2 = new Appointment(dentist, patient, service, start2);
            db.Appointments.Add(a2);
            await db.SaveChangesAsync();


            var handler = new RescheduleAppointment.Handler(db);
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => handler.Handle(new RescheduleAppointment.Command(a2.Id, start1), default));
        }


        private sealed class NoopPublisher : IPublisher
        {
            public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
                where TNotification : INotification
                => Task.CompletedTask;

            public Task Publish(object notification, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
