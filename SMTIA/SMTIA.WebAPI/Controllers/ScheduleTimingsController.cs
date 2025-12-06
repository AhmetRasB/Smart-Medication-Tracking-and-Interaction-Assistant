using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.ScheduleTimings.Create;
using SMTIA.Application.Features.ScheduleTimings.Delete;
using SMTIA.Application.Features.ScheduleTimings.GetByScheduleId;
using SMTIA.Application.Features.ScheduleTimings.Update;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class ScheduleTimingsController : ApiController
    {
        public ScheduleTimingsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateScheduleTimingCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("schedule/{scheduleId}")]
        public async Task<IActionResult> GetByScheduleId(Guid scheduleId, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetScheduleTimingsByScheduleIdQuery(scheduleId, userId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateScheduleTimingCommand request, CancellationToken cancellationToken)
        {
            var command = request with { Id = id };
            var response = await _mediator.Send(command, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteScheduleTimingCommand(id, userId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}

