using Application.Abstractions;
using Application.DTOs;
using Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Services
{
    public sealed record GetServicesQuery() : IRequest<List<ServiceDto>>;
    public sealed record CreateServiceCommand(string Name, int DurationMinutes) : IRequest<Guid>;
    public sealed record UpdateServiceCommand(Guid Id, string Name, int DurationMinutes) : IRequest;
    public sealed record DeleteServiceCommand(Guid Id) : IRequest;

    public sealed class GetServicesHandler(IApplicationDbContext db) : IRequestHandler<GetServicesQuery, List<ServiceDto>>
    {
        public async Task<List<ServiceDto>> Handle(GetServicesQuery request, CancellationToken ct)
            => await db.Services
               .OrderBy(x => x.Name)
               .Select(x => new ServiceDto(x.Id, x.Name, x.DurationMinutes))
               .ToListAsync(ct);
    }

    public sealed class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.DurationMinutes).GreaterThan(0);
        }
    }
    public sealed class CreateServiceHandler(IApplicationDbContext db) : IRequestHandler<CreateServiceCommand, Guid>
    {
        public async Task<Guid> Handle(CreateServiceCommand r, CancellationToken ct)
        {
            var e = new Service(r.Name, r.DurationMinutes);
            db.Services.Add(e);
            await db.SaveChangesAsync(ct);
            return e.Id;
        }
    }

    public sealed class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
    {
        public UpdateServiceValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.DurationMinutes).GreaterThan(0);
        }
    }
    public sealed class UpdateServiceHandler(IApplicationDbContext db) : IRequestHandler<UpdateServiceCommand>
    {
        public async Task Handle(UpdateServiceCommand r, CancellationToken ct)
        {
            var e = await db.Services.FindAsync([r.Id], ct) ?? throw new KeyNotFoundException("Service not found");
            e.Update(r.Name, r.DurationMinutes);
            await db.SaveChangesAsync(ct);
        }
    }

    public sealed class DeleteServiceHandler(IApplicationDbContext db) : IRequestHandler<DeleteServiceCommand>
    {
        public async Task Handle(DeleteServiceCommand r, CancellationToken ct)
        {
            var e = await db.Services.FindAsync([r.Id], ct) ?? throw new KeyNotFoundException("Service not found");
            db.Services.Remove(e);
            await db.SaveChangesAsync(ct);
        }
    }
}
