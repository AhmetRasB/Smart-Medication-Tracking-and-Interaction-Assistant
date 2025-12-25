using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SMTIA.Application.Features.Interactions.Analyze;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class InteractionsController : ApiController
    {
        public InteractionsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeInteractions(
            [FromBody] AnalyzeInteractionsRequest request,
            CancellationToken cancellationToken)
        {
            // Always use userId from JWT claims to prevent cross-user access
            var userIdClaim = User.FindFirst("Id") ?? User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Geçersiz kullanıcı kimliği (token)"); 
            }

            var command = new AnalyzeInteractionsCommand(
                userId,
                request.NewMedicineId,
                request.NewMedicineName);

            var response = await _mediator.Send(command, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }

    public sealed record AnalyzeInteractionsRequest(
        Guid? UserId,
        Guid? NewMedicineId,
        string? NewMedicineName);
}

