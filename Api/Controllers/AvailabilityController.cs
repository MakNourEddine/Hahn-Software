using Application.Features.Availability;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/availability")]
    public class AvailabilityController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<GetAvailability.Slot>>> Get([FromQuery] Guid dentistId, [FromQuery] DateOnly date)
        => Ok(await mediator.Send(new GetAvailability.Query(dentistId, date)));
    }
}
