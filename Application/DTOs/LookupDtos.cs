using Domain.Appointments;

namespace Application.DTOs
{
    public sealed record DentistDto(Guid Id, string FullName);
    public sealed record PatientDto(Guid Id, string FullName, string Email);
    public sealed record ServiceDto(Guid Id, string Name, int DurationMinutes);

    public sealed record AppointmentListItemDto(
        Guid Id,
        DateTimeOffset StartUtc,
        int DurationMinutes,
        AppointmentStatus Status,
        Guid PatientId,
        string PatientName,
        Guid ServiceId,
        string ServiceName);

}
