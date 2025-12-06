using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.IntakeLogs.Create;
using SMTIA.Application.Features.IntakeLogs.Delete;
using SMTIA.Application.Features.IntakeLogs.GetUserLogs;
using SMTIA.Application.Features.IntakeLogs.MarkAsTaken;
using SMTIA.Application.Features.IntakeLogs.Update;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class IntakeLogsController : ApiController
    {
        public IntakeLogsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIntakeLogCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLogs(
            Guid userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] Guid? scheduleId,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(
                new GetUserIntakeLogsQuery(userId, startDate, endDate, scheduleId),
                cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateIntakeLogCommand request, CancellationToken cancellationToken)
        {
            var command = request with { Id = id };
            var response = await _mediator.Send(command, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteIntakeLogCommand(id, userId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{logId}/markAsTaken")]
        public async Task<IActionResult> MarkAsTaken(
            Guid logId,
            [FromQuery] Guid userId,
            [FromBody] MarkIntakeAsTakenRequest request,
            CancellationToken cancellationToken)
        {
            var command = new MarkIntakeAsTakenCommand(
                logId,
                userId,
                request.IsTaken,
                request.IsSkipped,
                request.TakenTime,
                request.Notes);
            
            var response = await _mediator.Send(command, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }

    public sealed record MarkIntakeAsTakenRequest(
        bool IsTaken,
        bool IsSkipped,
        DateTime? TakenTime,
        string? Notes);
}

