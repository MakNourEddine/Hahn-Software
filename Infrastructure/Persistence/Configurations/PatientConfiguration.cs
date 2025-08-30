using Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Email).HasMaxLength(200).IsRequired();

            b.Ignore(x => x.DomainEvents);
        }
    }
}
