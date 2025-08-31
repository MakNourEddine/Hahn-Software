using Application.Abstractions;
using Domain.Appointments;
using Domain.Common;
using Domain.Dentists;
using Domain.Patients;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Dentist> Dentists => Set<Dentist>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<BaseEvent>();

            modelBuilder.Entity<Dentist>(b =>
            {
                b.ToTable("Dentists");
                b.HasKey(x => x.Id);
                b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                b.Ignore(x => x.DomainEvents);
            });

            modelBuilder.Entity<Patient>(b =>
            {
                b.ToTable("Patients");
                b.HasKey(x => x.Id);
                b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                b.Property(x => x.Email).HasMaxLength(256).IsRequired();
                b.Ignore(x => x.DomainEvents);
            });

            modelBuilder.Entity<Service>(b =>
            {
                b.ToTable("Services");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(150).IsRequired();
                b.Property(x => x.DurationMinutes).IsRequired();
                b.Ignore(x => x.DomainEvents);
            });

            modelBuilder.Entity<Appointment>(b =>
            {
                b.ToTable("Appointments");
                b.HasKey(x => x.Id);

                b.Property(x => x.StartUtc).IsRequired();
                b.Property(x => x.DurationMinutes).IsRequired();
                b.Property(x => x.Status).IsRequired();

                b.HasOne<Dentist>()
                    .WithMany()
                    .HasForeignKey(x => x.DentistId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne<Patient>()
                    .WithMany()
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne<Service>()
                    .WithMany()
                    .HasForeignKey(x => x.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasIndex(x => new { x.DentistId, x.StartUtc })
                    .IsUnique()
                    .HasFilter("[Status] = 0");

                b.HasIndex(x => new { x.DentistId, x.StartUtc });

                b.Ignore(x => x.DomainEvents);
            });

            // Dentists
            var d1 = new Guid("11111111-1111-1111-1111-111111111111"); // Dr. Alice Smith
            var d2 = new Guid("11111111-1111-1111-1111-222222222222"); // Dr. Bob Lee
            var d3 = new Guid("11111111-1111-1111-1111-333333333333"); // Dr. Carol Tan

            // Patients
            var p1 = new Guid("22222222-2222-2222-2222-111111111111"); // John Doe
            var p2 = new Guid("22222222-2222-2222-2222-222222222222"); // Jane Roe
            var p3 = new Guid("22222222-2222-2222-2222-333333333333"); // Omar Aziz
            var p4 = new Guid("22222222-2222-2222-2222-444444444444"); // Lina Haddad
            var p5 = new Guid("22222222-2222-2222-2222-555555555555"); // Mei Chen
            var p6 = new Guid("22222222-2222-2222-2222-666666666666"); // Carlos Ruiz

            // Services
            var s1 = new Guid("33333333-3333-3333-3333-111111111111"); // Cleaning 30
            var s2 = new Guid("33333333-3333-3333-3333-222222222222"); // Filling 60
            var s3 = new Guid("33333333-3333-3333-3333-333333333333"); // Whitening 45

            modelBuilder.Entity<Dentist>().HasData(
                new { Id = d1, FullName = "Dr. Alice Smith" },
                new { Id = d2, FullName = "Dr. Bob Lee" },
                new { Id = d3, FullName = "Dr. Carol Tan" }
            );

            modelBuilder.Entity<Patient>().HasData(
                new { Id = p1, FullName = "John Doe", Email = "john@example.com" },
                new { Id = p2, FullName = "Jane Roe", Email = "jane.roe@example.com" },
                new { Id = p3, FullName = "Omar Aziz", Email = "omar.aziz@example.com" },
                new { Id = p4, FullName = "Lina Haddad", Email = "lina.haddad@example.com" },
                new { Id = p5, FullName = "Mei Chen", Email = "mei.chen@example.com" },
                new { Id = p6, FullName = "Carlos Ruiz", Email = "carlos.ruiz@example.com" }
            );

            modelBuilder.Entity<Service>().HasData(
                new { Id = s1, Name = "Cleaning", DurationMinutes = 30 },
                new { Id = s2, Name = "Filling", DurationMinutes = 60 },
                new { Id = s3, Name = "Whitening", DurationMinutes = 45 }
            );

            // For d1 on 2025-09-01: 09:00 (30m), 09:30 (30m), 10:00 (60m)
            var a1 = new Guid("44444444-4444-4444-4444-111111111111");
            var a2 = new Guid("44444444-4444-4444-4444-222222222222");
            var a3 = new Guid("44444444-4444-4444-4444-333333333333");
            // For d2 on 2025-09-01: 09:00 (45m), 10:00 (60m)
            var a4 = new Guid("44444444-4444-4444-4444-444444444444");
            var a5 = new Guid("44444444-4444-4444-4444-555555555555");
            // For d3 on 2025-09-02: 11:00 (30m)
            var a6 = new Guid("44444444-4444-4444-4444-666666666666");

            modelBuilder.Entity<Appointment>().HasData(
                new
                {
                    Id = a1,
                    DentistId = d1,
                    PatientId = p1,
                    ServiceId = s1,
                    StartUtc = DateTimeOffset.Parse("2025-09-01T09:00:00Z"),
                    DurationMinutes = 30,
                    Status = AppointmentStatus.Scheduled
                },
                new
                {
                    Id = a2,
                    DentistId = d1,
                    PatientId = p2,
                    ServiceId = s1,
                    StartUtc = DateTimeOffset.Parse("2025-09-01T09:30:00Z"),
                    DurationMinutes = 30,
                    Status = AppointmentStatus.Scheduled
                },
                new
                {
                    Id = a3,
                    DentistId = d1,
                    PatientId = p3,
                    ServiceId = s2,
                    StartUtc = DateTimeOffset.Parse("2025-09-01T10:00:00Z"),
                    DurationMinutes = 60,
                    Status = AppointmentStatus.Scheduled
                },
                new
                {
                    Id = a4,
                    DentistId = d2,
                    PatientId = p4,
                    ServiceId = s3,
                    StartUtc = DateTimeOffset.Parse("2025-09-01T09:00:00Z"),
                    DurationMinutes = 45,
                    Status = AppointmentStatus.Scheduled
                },
                new
                {
                    Id = a5,
                    DentistId = d2,
                    PatientId = p5,
                    ServiceId = s2,
                    StartUtc = DateTimeOffset.Parse("2025-09-01T10:00:00Z"),
                    DurationMinutes = 60,
                    Status = AppointmentStatus.Scheduled
                },
                new
                {
                    Id = a6,
                    DentistId = d3,
                    PatientId = p6,
                    ServiceId = s1,
                    StartUtc = DateTimeOffset.Parse("2025-09-02T11:00:00Z"),
                    DurationMinutes = 30,
                    Status = AppointmentStatus.Scheduled
                }
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
                await publisher.Publish(domainEvent, ct);

            entities.ForEach(e => e.ClearDomainEvents());
            return result;
        }

        public override int SaveChanges()
            => SaveChangesAsync().GetAwaiter().GetResult();
    }
}
