using Domain.Appointments;
using Domain.Dentists;
using Domain.Patients;
using Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions
{
    public interface IApplicationDbContext
    {
        DbSet<Dentist> Dentists { get; }
        DbSet<Patient> Patients { get; }
        DbSet<Service> Services { get; }
        DbSet<Appointment> Appointments { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
