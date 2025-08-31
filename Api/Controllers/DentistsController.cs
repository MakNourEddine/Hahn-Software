using Application.DTOs;
using Application.Features.Dentists;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class DentistsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<DentistDto>>> GetAll() => await mediator.Send(new GetDentistsQuery());

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateDentistCommand cmd)
            => Ok(await mediator.Send(cmd));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDentistCommand body)
        {
            var cmd = body with { Id = id };
            await mediator.Send(cmd);
            return NoContent();
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteDentistCommand(id)); return NoContent();
        }
    }
}
