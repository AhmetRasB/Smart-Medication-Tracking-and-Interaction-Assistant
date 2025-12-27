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
                // Use a transaction to ensure atomicity
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Load user - track it for updates
                    var user = await dbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == userGuid, cancellationToken);

                    if (user is null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return NotFound(new { message = "User not found" });
                    }

                    // 1. Update Scalar Fields
                    _logger.LogInformation("Step 1: Updating Scalar Fields. User ConcurrencyStamp: {Stamp}", user.ConcurrencyStamp);
                    
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

                    // 2. Handle Collections - Remove existing items directly from DbSet
                    _logger.LogInformation("Step 2: Clearing Collections");
                    
                    // Remove existing emergency contacts
                    var existingContacts = await dbContext.UserEmergencyContacts
                        .Where(ec => ec.UserId == userGuid)
                        .ToListAsync(cancellationToken);
                    if (existingContacts.Any())
                    {
                        dbContext.UserEmergencyContacts.RemoveRange(existingContacts);
                    }

                    // Remove existing surgeries
                    var existingSurgeries = await dbContext.UserSurgeries
                        .Where(s => s.UserId == userGuid)
                        .ToListAsync(cancellationToken);
                    if (existingSurgeries.Any())
                    {
                        dbContext.UserSurgeries.RemoveRange(existingSurgeries);
                    }

                    // Remove existing diseases
                    var existingDiseases = await dbContext.UserDiseases
                        .Where(d => d.UserId == userGuid)
                        .ToListAsync(cancellationToken);
                    if (existingDiseases.Any())
                    {
                        dbContext.UserDiseases.RemoveRange(existingDiseases);
                    }

                    // 3. Add New Items
                    _logger.LogInformation("Step 3: Adding New Items");
                    
                    if (request.EmergencyContacts != null && request.EmergencyContacts.Any())
                    {
                        _logger.LogInformation("Adding {Count} EmergencyContacts...", request.EmergencyContacts.Count);
                        foreach (var contact in request.EmergencyContacts)
                        {
                            var newContact = new UserEmergencyContact
                            {
                                Id = Guid.NewGuid(),
                                Name = contact.Name,
                                Phone = contact.Phone,
                                Relationship = contact.Relationship,
                                UserId = userGuid
                            };
                            dbContext.UserEmergencyContacts.Add(newContact);
                        }
                    }

                    if (request.Surgeries != null && request.Surgeries.Any())
                    {
                        _logger.LogInformation("Adding {Count} Surgeries...", request.Surgeries.Count);
                        foreach (var surgeryName in request.Surgeries)
                        {
                            var newSurgery = new UserSurgery
                            {
                                Id = Guid.NewGuid(),
                                SurgeryName = surgeryName,
                                UserId = userGuid
                            };
                            dbContext.UserSurgeries.Add(newSurgery);
                        }
                    }

                    if (request.ChronicDiseases != null && request.ChronicDiseases.Any())
                    {
                        _logger.LogInformation("Adding {Count} ChronicDiseases...", request.ChronicDiseases.Count);
                        foreach (var diseaseName in request.ChronicDiseases)
                        {
                            var newDisease = new UserDisease
                            {
                                Id = Guid.NewGuid(),
                                DiseaseName = diseaseName,
                                UserId = userGuid
                            };
                            dbContext.UserDiseases.Add(newDisease);
                        }
                    }

                    // Save all changes in a single transaction - this updates ConcurrencyStamp only once
                    _logger.LogInformation("Saving all changes in single transaction...");
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    
                    _logger.LogInformation("UpdateHealth completed successfully");
                    return Ok(new { message = "Health info updated successfully" });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError("CONCURRENCY EXCEPTION CAUGHT!");
                    foreach (var entry in ex.Entries)
                    {
                        var entityName = entry.Entity.GetType().Name;
                        var state = entry.State;
                        var dbValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                        _logger.LogError("Conflict Entity: {EntityName}, State: {State}", entityName, state);
                        
                        if (entry.Entity is AppUser appUser)
                        {
                             var clientStamp = appUser.ConcurrencyStamp;
                             var dbStamp = dbValues?.GetValue<string>("ConcurrencyStamp");
                             _logger.LogError("AppUser Conflict: ClientStamp={ClientStamp}, DbStamp={DbStamp}", clientStamp, dbStamp);
                        }
                    }

                    if (i == maxRetries - 1)
                    {
                        _logger.LogError("UpdateHealth failed after {MaxRetries} attempts due to concurrency.", maxRetries);
                        return Conflict(new { message = "Data was modified by another process. Please reload and try again." });
                    }
                    // Clear change tracker to avoid stale entries in next iteration
                    dbContext.ChangeTracker.Clear();
                    _logger.LogWarning("UpdateHealth concurrency exception, retrying... Attempt {Attempt}", i + 1);
                    
                    // Wait a bit before retrying to allow other operations to complete
                    await Task.Delay(100 * (i + 1), cancellationToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Unexpected error in UpdateHealth");
                    throw;
                }
            }

            return Conflict(new { message = "Update failed." });
        }

        public sealed record UpdateHealthRequest(
            [property: System.Text.Json.Serialization.JsonPropertyName("tcIdentityNo")] string? TcIdentityNo,
            [property: System.Text.Json.Serialization.JsonPropertyName("bloodType")] string? BloodType,
            [property: System.Text.Json.Serialization.JsonPropertyName("smokes")] bool? Smokes,
            [property: System.Text.Json.Serialization.JsonPropertyName("cigarettesPerDay")] int? CigarettesPerDay,
            [property: System.Text.Json.Serialization.JsonPropertyName("cigarettesUnit")] string? CigarettesUnit,
            [property: System.Text.Json.Serialization.JsonPropertyName("drinksAlcohol")] bool? DrinksAlcohol,
            [property: System.Text.Json.Serialization.JsonPropertyName("hadCovid")] bool? HadCovid,
            [property: System.Text.Json.Serialization.JsonPropertyName("birthCity")] string? BirthCity,
            [property: System.Text.Json.Serialization.JsonPropertyName("acilNot")] string? AcilNot,
            [property: System.Text.Json.Serialization.JsonPropertyName("handedness")] string? Handedness,
            [property: System.Text.Json.Serialization.JsonPropertyName("emergencyContacts")] List<EmergencyContactDto>? EmergencyContacts,
            [property: System.Text.Json.Serialization.JsonPropertyName("surgeries")] List<string>? Surgeries,
            [property: System.Text.Json.Serialization.JsonPropertyName("chronicDiseases")] List<string>? ChronicDiseases
        );

        public sealed record EmergencyContactDto(
            [property: System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
            [property: System.Text.Json.Serialization.JsonPropertyName("phone")] string Phone,
            [property: System.Text.Json.Serialization.JsonPropertyName("relationship")] string? Relationship
        );
        
        public sealed record UpdateProfileRequest(
            string? Name,
            int? AgeYears,
            int? HeightCm,
            decimal? WeightKg,
            string? Gender
        );
    }
}
