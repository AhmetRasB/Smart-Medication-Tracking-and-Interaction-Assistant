using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.MedicineMappings.Confirm;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class MedicineMappingsController : ApiController
    {
        public MedicineMappingsController(IMediator mediator) : base(mediator)
        {
        }

        // Self-learning: user confirms/rejects Gemma suggestion
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(
            [FromBody] ConfirmMedicineMappingRequest request,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst("Id") ?? User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Geçersiz kullanıcı kimliği (token)");
            }

            var cmd = new ConfirmMedicineMappingCommand(userId, request.MappingId, request.Confirmed);
            var response = await _mediator.Send(cmd, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }

    public sealed record ConfirmMedicineMappingRequest(
        Guid MappingId,
        bool Confirmed);
}


