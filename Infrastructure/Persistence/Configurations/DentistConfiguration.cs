using Domain.Dentists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DentistConfiguration : IEntityTypeConfiguration<Dentist>
    {
        public void Configure(EntityTypeBuilder<Dentist> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FullName).HasMaxLength(100).IsRequired();

            b.Ignore(x => x.DomainEvents);
        }
    }
}
