using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class ProfileController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IMediator mediator, UserManager<AppUser> userManager, ILogger<ProfileController> logger) : base(mediator)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound(new { message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);

            int? ageYears = null;
            if (user.DateOfBirth.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                var dob = user.DateOfBirth.Value.Date;
                var age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--;
                ageYears = Math.Max(age, 0);
            }

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                name = user.FullName,
                userName = user.UserName,
                ageYears,
                heightCm = user.HeightCm,
                weightKg = user.Weight,
                gender = user.Gender,
                bloodType = user.BloodType,
                roles = roles.ToList()
            });
        }
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound(new { message = "User not found" });

            if (request.HeightCm.HasValue) user.HeightCm = request.HeightCm.Value;
            if (request.WeightKg.HasValue) user.Weight = request.WeightKg.Value;
            if (!string.IsNullOrWhiteSpace(request.Gender)) user.Gender = request.Gender;
            if (request.AgeYears.HasValue)
            {
                // Approximate DOB update if age changes
                var today = DateTime.UtcNow.Date;
                user.DateOfBirth = new DateTime(today.Year - request.AgeYears.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                // Split name if needed or just update FullName logic if it exists (AppUser usually has FirstName/LastName)
                // Assuming Name maps to FirstName for simplicity or we need to split it
                var parts = request.Name.Trim().Split(' ', 2);
                user.FirstName = parts[0];
                if (parts.Length > 1) user.LastName = parts[1];
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to update profile", errors = result.Errors });
            }

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var user = await _userManager.Users
                .Include(u => u.EmergencyContacts)
                .Include(u => u.UserSurgeries)
                .Include(u => u.UserDiseases)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId), cancellationToken);

            if (user is null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                tcIdentityNo = user.TcIdentityNo,
                bloodType = user.BloodType,
                smokes = user.Smokes,
                cigarettesPerDay = user.CigarettesPerDay,
                cigarettesUnit = user.CigarettesUnit,
                drinksAlcohol = user.DrinksAlcohol,
                hadCovid = user.HadCovid,
                birthCity = user.BirthCity,
                acilNot = user.AcilNot,
                handedness = user.Handedness,
                emergencyContacts = user.EmergencyContacts.Select(c => new { c.Name, c.Phone, c.Relationship }).ToList(),
                surgeries = user.UserSurgeries.Select(s => s.SurgeryName).ToList(),
                chronicDiseases = user.UserDiseases.Select(d => d.DiseaseName).ToList()
            });
        }

        [HttpPut("health")]
        public async Task<IActionResult> UpdateHealth([FromBody] UpdateHealthRequest request, [FromServices] SMTIA.Infrastructure.Context.ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("UpdateHealth called with request: {@Request}", request);

            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Missing user id claim" });

            var userGuid = Guid.Parse(userId);
            int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Load user with collections
                    var user = await dbContext.Users
                        .Include(u => u.EmergencyContacts)
                        .Include(u => u.UserSurgeries)
                        .Include(u => u.UserDiseases)
                        .FirstOrDefaultAsync(u => u.Id == userGuid, cancellationToken);

                    if (user is null) return NotFound(new { message = "User not found" });

                    // 1. Update Scalar Fields
                    user.TcIdentityNo = request.TcIdentityNo;
                    user.BloodType = request.BloodType;
                    user.Smokes = request.Smokes;
                    user.CigarettesPerDay = request.CigarettesPerDay;
                    user.CigarettesUnit = request.CigarettesUnit;
                    user.DrinksAlcohol = request.DrinksAlcohol;
                    user.HadCovid = request.HadCovid;
                    user.BirthCity = request.BirthCity;
                    user.AcilNot = request.AcilNot;
                    user.Handedness = request.Handedness;

                    _logger.LogInformation("Saving scalar updates...");
                    await dbContext.SaveChangesAsync(cancellationToken);

                    // 2. Clear Collections
                    _logger.LogInformation("Clearing collections...");
                    user.EmergencyContacts.Clear();
                    user.UserSurgeries.Clear();
                    user.UserDiseases.Clear();
                    
                    _logger.LogInformation("Saving collection clearance...");
                    await dbContext.SaveChangesAsync(cancellationToken);

                    // 3. Add New Items
                    if (request.EmergencyContacts != null && request.EmergencyContacts.Any())
                    {
                        _logger.LogInformation("Adding {Count} EmergencyContacts...", request.EmergencyContacts.Count);
                        foreach (var contact in request.EmergencyContacts)
                        {
                            user.EmergencyContacts.Add(new UserEmergencyContact
                            {
                                Id = Guid.NewGuid(),
                                Name = contact.Name,
                                Phone = contact.Phone,
                                Relationship = contact.Relationship,
                                UserId = userGuid
                            });
                        }
                    }

                    if (request.Surgeries != null && request.Surgeries.Any())
                    {
                        _logger.LogInformation("Adding {Count} Surgeries...", request.Surgeries.Count);
                        foreach (var surgeryName in request.Surgeries)
                        {
                            user.UserSurgeries.Add(new UserSurgery
                            {
                                Id = Guid.NewGuid(),
                                SurgeryName = surgeryName,
                                UserId = userGuid
                            });
                        }
                    }

                    if (request.ChronicDiseases != null && request.ChronicDiseases.Any())
                    {
                        _logger.LogInformation("Adding {Count} ChronicDiseases...", request.ChronicDiseases.Count);
                        foreach (var diseaseName in request.ChronicDiseases)
                        {
                            user.UserDiseases.Add(new UserDisease
                            {
                                Id = Guid.NewGuid(),
                                DiseaseName = diseaseName,
                                UserId = userGuid
                            });
                        }
                    }

                    _logger.LogInformation("Saving new collection items...");
                    await dbContext.SaveChangesAsync(cancellationToken);
                    
                    return Ok(new { message = "Health info updated successfully" });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        _logger.LogError("Concurrency conflict for entity: {EntityName}, State: {State}", entry.Entity.GetType().Name, entry.State);
                    }

                    if (i == maxRetries - 1)
                    {
                        _logger.LogError("UpdateHealth failed after {MaxRetries} attempts due to concurrency.", maxRetries);
                        return Conflict(new { message = "Data was modified by another process. Please reload and try again." });
                    }
                    // Clear change tracker to avoid stale entries in next iteration
                    dbContext.ChangeTracker.Clear();
                    _logger.LogWarning("UpdateHealth concurrency exception, retrying... Attempt {Attempt}", i + 1);
                }
            }

            return Conflict(new { message = "Update failed." });
        }

        public sealed record UpdateHealthRequest(
            string? TcIdentityNo,
            string? BloodType,
            bool? Smokes,
            int? CigarettesPerDay,
            string? CigarettesUnit,
            bool? DrinksAlcohol,
            bool? HadCovid,
            string? BirthCity,
            string? AcilNot,
            string? Handedness,
            List<EmergencyContactDto>? EmergencyContacts,
            List<string>? Surgeries,
            List<string>? ChronicDiseases
        );

        public sealed record EmergencyContactDto(string Name, string Phone, string? Relationship);
        
        public sealed record UpdateProfileRequest(
            string? Name,
            int? AgeYears,
            int? HeightCm,
            decimal? WeightKg,
            string? Gender
        );
    }
}
