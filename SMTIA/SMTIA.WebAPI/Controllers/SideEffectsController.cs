using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class SideEffectsController : ApiController
    {
        private readonly ApplicationDbContext _dbContext;

        public SideEffectsController(IMediator mediator, ApplicationDbContext dbContext) : base(mediator)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMySideEffects(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var userGuid = Guid.Parse(userId);

            var sideEffects = await _dbContext.UserSideEffects
                .Where(s => s.UserId == userGuid && !s.IsDeleted)
                .OrderByDescending(s => s.Date)
                .ToListAsync(cancellationToken);

            var result = sideEffects.Select(s => new
            {
                s.Id,
                medicine = new { name = s.MedicineName, id = s.MedicineId },
                s.Severity,
                sideEffects = !string.IsNullOrEmpty(s.SideEffects)
                    ? s.SideEffects.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()
                    : new List<string>(),
                s.Date
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddSideEffect([FromBody] AddSideEffectRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var userGuid = Guid.Parse(userId);

            var sideEffect = new UserSideEffect
            {
                UserId = userGuid,
                MedicineId = request.MedicineId,
                MedicineName = request.MedicineName ?? "Unknown",
                Severity = request.Severity ?? "mild",
                SideEffects = request.SideEffects != null ? string.Join(",", request.SideEffects) : string.Empty,
                Date = request.Date ?? DateTime.UtcNow
            };

            _dbContext.UserSideEffects.Add(sideEffect);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Side effect added successfully", id = sideEffect.Id });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSideEffect(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var userGuid = Guid.Parse(userId);

            var sideEffect = await _dbContext.UserSideEffects
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userGuid, cancellationToken);

            if (sideEffect is null)
                return NotFound(new { message = "Side effect not found" });

            // Soft Delete
            sideEffect.IsDeleted = true;
            sideEffect.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Side effect deleted successfully" });
        }

        public sealed record AddSideEffectRequest(
            Guid? MedicineId,
            string? MedicineName,
            string? Severity,
            List<string>? SideEffects,
            DateTime? Date
        );
    }
}
