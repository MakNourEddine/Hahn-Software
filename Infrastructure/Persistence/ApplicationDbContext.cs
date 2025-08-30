using Application.Abstractions;
using Domain.Appointments;
using Domain.Common;
using Domain.Dentists;
using Domain.Patients;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IPublisher _publisher;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) : base(options)
        => _publisher = publisher;


        public DbSet<Dentist> Dentists => Set<Dentist>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Appointment> Appointments => Set<Appointment>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<BaseEvent>();

            var dentistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var patientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var cleaningId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var fillingId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<Dentist>().HasData(new { Id = dentistId, FullName = "Dr. Alice Smith" });

            modelBuilder.Entity<Patient>().HasData(new { Id = patientId, FullName = "John Doe", Email = "john@example.com" });

            modelBuilder.Entity<Service>().HasData(
                new { Id = cleaningId, Name = "Cleaning", DurationMinutes = 30 },
                new { Id = fillingId, Name = "Filling", DurationMinutes = 60 }
            );

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }


        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var entities = ChangeTracker.Entries<BaseEntity>().Select(e => e.Entity).ToList();
            var events = entities.SelectMany(e => e.DomainEvents).ToList();
            var result = await base.SaveChangesAsync(ct);
            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, ct);
            entities.ForEach(e => e.ClearDomainEvents());
            return result;
        }
    }
}
