using Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> b)
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.StartUtc).IsRequired();
            b.Property(x => x.DurationMinutes).IsRequired();

            b.HasOne(x => x.Dentist)
                .WithMany()
                .HasForeignKey(x => x.DentistId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Service)
                .WithMany()
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.DentistId, x.StartUtc });
            b.HasIndex(x => new { x.PatientId, x.StartUtc });

            b.Ignore(x => x.DomainEvents);
        }
    }
}
