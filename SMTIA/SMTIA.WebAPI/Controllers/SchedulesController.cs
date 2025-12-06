using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.Schedules.Create;
using SMTIA.Application.Features.Schedules.Delete;
using SMTIA.Application.Features.Schedules.GetById;
using SMTIA.Application.Features.Schedules.GetUserSchedules;
using SMTIA.Application.Features.Schedules.Update;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class SchedulesController : ApiController
    {
        public SchedulesController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateScheduleCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetScheduleByIdQuery(id, userId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSchedules(Guid userId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetUserSchedulesQuery(userId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateScheduleCommand request, CancellationToken cancellationToken)
        {
            var command = request with { Id = id };
            var response = await _mediator.Send(command, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteScheduleCommand(id, userId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}

