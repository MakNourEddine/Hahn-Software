using Application.Features.Appointments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController(IMediator mediator) : ControllerBase
    {
        [HttpPost("book")]
        public async Task<ActionResult<Guid>> Book([FromBody] BookAppointment.Command cmd)
        => Ok(await mediator.Send(cmd));


        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult> Cancel([FromRoute] Guid id, [FromBody] CancelAppointment.Command body)
        {
            await mediator.Send(new CancelAppointment.Command(id, body.Reason));
            return NoContent();
        }


        [HttpPost("{id:guid}/reschedule")]
        public async Task<ActionResult> Reschedule([FromRoute] Guid id, [FromBody] RescheduleAppointment.Command body)
        {
            await mediator.Send(new RescheduleAppointment.Command(id, body.NewStartUtc));
            return NoContent();
        }


        [HttpGet("by-dentist")]
        public async Task<ActionResult<IReadOnlyList<ListAppointmentsByDentist.Dto>>> ByDentist([FromQuery] Guid dentistId, [FromQuery] DateOnly? date)
        => Ok(await mediator.Send(new ListAppointmentsByDentist.Query(dentistId, date)));
    }
}
