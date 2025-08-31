using Application.Abstractions;
using Application.DTOs;
using Domain.Dentists;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Dentists
{
    public sealed record GetDentistsQuery() : IRequest<List<DentistDto>>;
    public sealed record CreateDentistCommand(string FullName) : IRequest<Guid>;
    public sealed record UpdateDentistCommand(Guid Id, string FullName) : IRequest;
    public sealed record DeleteDentistCommand(Guid Id) : IRequest;

    public sealed class GetDentistsHandler(IApplicationDbContext db) : IRequestHandler<GetDentistsQuery, List<DentistDto>>
    {
        public async Task<List<DentistDto>> Handle(GetDentistsQuery request, CancellationToken ct)
            => await db.Dentists
               .OrderBy(x => x.FullName)
               .Select(x => new DentistDto(x.Id, x.FullName))
               .ToListAsync(ct);
    }

    public sealed class CreateDentistValidator : AbstractValidator<CreateDentistCommand>
    {
        public CreateDentistValidator() => RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
    public sealed class CreateDentistHandler(IApplicationDbContext db) : IRequestHandler<CreateDentistCommand, Guid>
    {
        public async Task<Guid> Handle(CreateDentistCommand r, CancellationToken ct)
        {
            var e = new Dentist(r.FullName);
            db.Dentists.Add(e);
            await db.SaveChangesAsync(ct);
            return e.Id;
        }
    }

    public sealed class UpdateDentistValidator : AbstractValidator<UpdateDentistCommand>
    {
        public UpdateDentistValidator() => RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
    public sealed class UpdateDentistHandler(IApplicationDbContext db) : IRequestHandler<UpdateDentistCommand>
    {
        public async Task Handle(UpdateDentistCommand r, CancellationToken ct)
        {
            var e = await db.Dentists.FindAsync([r.Id], ct) ?? throw new KeyNotFoundException("Dentist not found");
            e.Rename(r.FullName);

            await db.SaveChangesAsync(ct);
        }
    }

    public sealed class DeleteDentistHandler(IApplicationDbContext db) : IRequestHandler<DeleteDentistCommand>
    {
        public async Task Handle(DeleteDentistCommand r, CancellationToken ct)
        {
            var e = await db.Dentists.FindAsync([r.Id], ct) ?? throw new KeyNotFoundException("Dentist not found");
            db.Dentists.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }
}
