using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.Calendar.GetCalendar;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class CalendarController : ApiController
    {
        public CalendarController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendar(
            [FromQuery] Guid userId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(
                new GetCalendarQuery(userId, startDate, endDate),
                cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}

