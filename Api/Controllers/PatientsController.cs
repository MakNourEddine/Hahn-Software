using Application.DTOs;
using Application.Features.Patients;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PatientsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<PatientDto>>> GetAll()
            => Ok(await mediator.Send(new GetPatientsQuery()));

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePatientCommand cmd)
            => Ok(await mediator.Send(cmd));

        public sealed record UpdatePatientBody(string FullName, string Email);

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientBody body)
        {
            await mediator.Send(new UpdatePatientCommand(id, body.FullName, body.Email));
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeletePatientCommand(id));
            return NoContent();
        }
    }
}
