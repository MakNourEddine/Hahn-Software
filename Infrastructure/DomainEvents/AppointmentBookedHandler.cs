using Domain.Appointments.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DomainEvents
{
    public sealed class AppointmentBookedHandler(ILogger<AppointmentBookedHandler> logger) : INotificationHandler<AppointmentBookedEvent>
    {
        public Task Handle(AppointmentBookedEvent notification, CancellationToken ct)
        {
            logger.LogInformation("Appointment {Id} booked for dentist {DentistId} at {Start}", notification.AppointmentId, notification.DentistId, notification.StartUtc);
            return Task.CompletedTask;
        }
    }
}
