using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.Medicines.AddMedicineToUser;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class MedicinesController : ApiController
    {
        public MedicinesController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("add-to-user")]
        public async Task<IActionResult> AddMedicineToUser(AddMedicineToUserCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}

