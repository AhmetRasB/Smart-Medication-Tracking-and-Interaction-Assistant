using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.Fda.GetMedicineDetails;
using SMTIA.Application.Features.Fda.SearchMedicines;
using SMTIA.Application.Features.Medicines.AddMedicineToUser;
using SMTIA.Application.Features.Medicines.Create;
using SMTIA.Application.Features.Medicines.Delete;
using SMTIA.Application.Features.Medicines.GetAll;
using SMTIA.Application.Features.Medicines.GetById;
using SMTIA.Application.Features.Medicines.Update;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class MedicinesController : ApiController
    {
        public MedicinesController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("fda/search")]
        public async Task<IActionResult> SearchFdaMedicines([FromQuery] string searchTerm, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new SearchMedicinesQuery(searchTerm, limit), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("fda/{labelId}")]
        public async Task<IActionResult> GetFdaMedicineDetails(string labelId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetMedicineDetailsQuery(labelId), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMedicineCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetMedicineByIdQuery(id), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllMedicinesQuery(), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateMedicineCommand request, CancellationToken cancellationToken)
        {
            var command = request with { Id = id };
            var response = await _mediator.Send(command, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteMedicineCommand(id), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("add-to-user")]
        public async Task<IActionResult> AddMedicineToUser(AddMedicineToUserCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}

