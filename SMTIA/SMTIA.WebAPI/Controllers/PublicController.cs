using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Application.Features.Medicines.Search;
using SMTIA.Domain.Entities;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    // Simple public endpoints for frontend UX (email availability etc.)
    [AllowAnonymous]
    public sealed class PublicController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public PublicController(MediatR.IMediator mediator, UserManager<AppUser> userManager) : base(mediator)
        {
            _userManager = userManager;
        }

        [HttpGet("email-available")]
        public async Task<IActionResult> IsEmailAvailable([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { message = "Email is required" });

            var normalized = email.Trim();
            var user = await _userManager.FindByEmailAsync(normalized);
            return Ok(new { email = normalized, available = user == null });
        }

        // Public medicine search for onboarding UI (no token yet)
        [HttpGet("medicines/search")]
        public async Task<IActionResult> SearchMedicines([FromQuery] string query, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new SearchLocalMedicinesQuery(query, limit), cancellationToken);
            return StatusCode(response.StatusCode, response);
        }
    }
}


