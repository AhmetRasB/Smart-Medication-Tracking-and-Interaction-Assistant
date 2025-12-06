using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.AuditLogs.GetUserLogs;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class AuditLogsController : ApiController
    {
        public AuditLogsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLogs(
            Guid userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? action,
            [FromQuery] string? entityType,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(
                new GetUserAuditLogsQuery(userId, startDate, endDate, action, entityType),
                cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}

