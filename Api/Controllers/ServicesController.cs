using Application.DTOs;
using Application.Features.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ServicesController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ServiceDto>>> GetAll()
            => Ok(await mediator.Send(new GetServicesQuery()));

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateServiceCommand cmd)
            => Ok(await mediator.Send(cmd));

        public sealed record UpdateServiceBody(string Name, int DurationMinutes);

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceBody body)
        {
            await mediator.Send(new UpdateServiceCommand(id, body.Name, body.DurationMinutes));
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteServiceCommand(id));
            return NoContent();
        }
    }
}
