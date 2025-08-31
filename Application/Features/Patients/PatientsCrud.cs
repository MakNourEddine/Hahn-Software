using Application.Abstractions;
using Application.DTOs;
using Domain.Patients;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Patients
{
    public sealed record GetPatientsQuery() : IRequest<List<PatientDto>>;
    public sealed record CreatePatientCommand(string FullName, string Email) : IRequest<Guid>;
    public sealed record UpdatePatientCommand(Guid Id, string FullName, string Email) : IRequest;
    public sealed record DeletePatientCommand(Guid Id) : IRequest;

    public sealed class GetPatientsHandler(IApplicationDbContext db) : IRequestHandler<GetPatientsQuery, List<PatientDto>>
    {
        public async Task<List<PatientDto>> Handle(GetPatientsQuery request, CancellationToken ct)
            => await db.Patients
               .OrderBy(x => x.FullName)
               .Select(x => new PatientDto(x.Id, x.FullName, x.Email))
               .ToListAsync(ct);
    }

    public sealed class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        }
    }
    public sealed class CreatePatientHandler(IApplicationDbContext db) : IRequestHandler<CreatePatientCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePatientCommand r, CancellationToken ct)
        {
            var e = new Patient(r.FullName, r.Email);
            db.Patients.Add(e);
            await db.SaveChangesAsync(ct);
            return e.Id;
        }
    }

    public sealed class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
    {
        public UpdatePatientValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        }
    }
    public sealed class UpdatePatientHandler(IApplicationDbContext db) : IRequestHandler<UpdatePatientCommand>
    {
        public async Task Handle(UpdatePatientCommand r, CancellationToken ct)
        {
            var e = await db.Patients.FindAsync([r.Id], ct) ?? throw new KeyNotFoundException("Patient not found");
            e.Update(r.FullName, r.Email);
            await db.SaveChangesAsync(ct);
        }
    }

    public sealed class DeletePatientHandler(IApplicationDbContext db) : IRequestHandler<DeletePatientCommand>
    {
        public async Task Handle(DeletePatientCommand r, CancellationToken ct)
        {
            var e = await db.Patients.FindAsync([r.Id], ct) ?? throw new KeyNotFoundException("Patient not found");
            db.Patients.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }
}
