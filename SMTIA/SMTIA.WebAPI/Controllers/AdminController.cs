using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public sealed class AdminController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _env;

        public AdminController(
            MediatR.IMediator mediator,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationDbContext dbContext,
            ILogger<AdminController> logger,
            IWebHostEnvironment env) : base(mediator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _logger = logger;
            _env = env;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = _userManager.Users.ToList();

            var result = new List<UserAdminDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                
                // Get counts from database
                var prescriptionCount = await _dbContext.UserPrescriptions
                    .CountAsync(p => p.UserId == u.Id, cancellationToken);
                
                var medicineCount = await _dbContext.MedicationSchedules
                    .Where(m => m.Prescription.UserId == u.Id)
                    .CountAsync(cancellationToken);
                
                result.Add(new UserAdminDto(
                    u.Id,
                    u.UserName ?? "",
                    u.Email ?? "",
                    u.FullName,
                    u.EmailConfirmed,
                    null, // CreatedAt - IdentityUser doesn't have this by default
                    u.DateOfBirth,
                    u.HeightCm,
                    u.Weight,
                    u.Gender,
                    u.BloodType,
                    roles.ToList(),
                    prescriptionCount,
                    medicineCount,
                    u.LockoutEnd));
            }

            return Ok(result.OrderBy(x => x.UserName));
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            // Only expose the 2 supported roles
            var roles = _roleManager.Roles
                .Select(r => r.Name!)
                .Where(r => r == "Admin" || r == "User")
                .OrderBy(x => x)
                .ToList();
            return Ok(roles);
        }

        [HttpPut("users/{userId:guid}/roles")]
        public async Task<IActionResult> SetUserRoles(Guid userId, [FromBody] SetUserRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return NotFound(new { message = "User not found" });

            var desiredRoles = (request.Roles ?? new List<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Only allow Admin/User roles
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Admin", "User" };
            desiredRoles = desiredRoles.Where(r => allowed.Contains(r)).ToList();

            // Ensure base roles exist
            foreach (var role in allowed)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));

            var currentRoles = await _userManager.GetRolesAsync(user);

            var toRemove = currentRoles.Where(r => !desiredRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
            var toAdd = desiredRoles.Where(r => !currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

            if (toRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);
            }

            if (toAdd.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addResult.Succeeded) return BadRequest(addResult.Errors);
            }

            return Ok(new
            {
                userId = user.Id,
                roles = await _userManager.GetRolesAsync(user)
            });
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAllAuditLogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? action,
            [FromQuery] string? entityType,
            [FromQuery] Guid? userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            if (startDate.HasValue)
            {
                var start = startDate.Value;
                if (start.Kind == DateTimeKind.Unspecified)
                    start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                else
                    start = start.ToUniversalTime();
                query = query.Where(a => a.CreatedAt >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value;
                if (end.Kind == DateTimeKind.Unspecified)
                    end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
                else
                    end = end.ToUniversalTime();
                query = query.Where(a => a.CreatedAt <= end);
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(a => a.Action.Contains(action));
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a => a.EntityType.Contains(entityType));
            }

            var total = await query.CountAsync(cancellationToken);
            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto(
                    a.Id,
                    a.UserId,
                    a.User.UserName ?? a.User.Email ?? "Unknown",
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    a.RequestPath,
                    a.RequestMethod,
                    a.ResponseStatus,
                    a.IpAddress,
                    a.CreatedAt,
                    a.AdditionalData))
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                logs,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        [HttpGet("serilog-logs")]
        public IActionResult GetSerilogLogs([FromQuery] int lines = 1000)
        {
            try
            {
                var logsDir = Path.Combine(_env.ContentRootPath, "logs");
                if (!Directory.Exists(logsDir))
                {
                    return Ok(new { logs = new List<string>(), message = "Logs directory not found" });
                }

                var logFiles = Directory.GetFiles(logsDir, "smtia-*.txt")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .ToList();

                if (logFiles.Count == 0)
                {
                    return Ok(new { logs = new List<string>(), message = "No log files found" });
                }

                var allLines = new List<string>();
                foreach (var file in logFiles.Take(5)) // Last 5 log files
                {
                    try
                    {
                        var fileLines = System.IO.File.ReadAllLines(file);
                        allLines.AddRange(fileLines);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reading log file: {File}", file);
                    }
                }

                // Get last N lines
                var recentLines = allLines
                    .TakeLast(lines)
                    .ToList();

                return Ok(new
                {
                    logs = recentLines,
                    totalLines = allLines.Count,
                    filesRead = logFiles.Take(5).Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Serilog files");
                return StatusCode(500, new { message = "Error reading log files", error = ex.Message });
            }
        }

        [HttpDelete("users/{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return NotFound(new { message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to delete user", errors = result.Errors });
            }

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPut("users/{userId:guid}/lockout")]
        public async Task<IActionResult> ToggleUserLockout(Guid userId, [FromBody] LockoutRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return NotFound(new { message = "User not found" });

            if (request.LockoutEnabled)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Lock indefinitely
            }
            else
            {
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to update lockout", errors = result.Errors });
            }

            return Ok(new
            {
                message = request.LockoutEnabled ? "User locked" : "User unlocked",
                lockoutEnabled = request.LockoutEnabled
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
            var totalAdmins = await _userManager.GetUsersInRoleAsync("Admin");
            var totalMedicines = await _dbContext.Medicines.CountAsync(cancellationToken);
            var totalAuditLogs = await _dbContext.AuditLogs.CountAsync(cancellationToken);
            var recentLogs = await _dbContext.AuditLogs
                .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync(cancellationToken);

            return Ok(new
            {
                totalUsers,
                totalAdmins = totalAdmins.Count,
                totalMedicines,
                totalAuditLogs,
                recentLogsLast7Days = recentLogs
            });
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username, email, and password are required" });
            }

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
            {
                return BadRequest(new { message = "User with this username already exists" });
            }

            // Create new admin user
            var user = new AppUser
            {
                UserName = request.UserName.Trim(),
                Email = request.Email.Trim(),
                FirstName = request.FirstName?.Trim() ?? "Admin",
                LastName = request.LastName?.Trim() ?? "User",
                EmailConfirmed = request.EmailConfirmed ?? true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Failed to create user: {errors}" });
            }

            // Add Admin role
            var addRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addRoleResult.Succeeded)
            {
                // If role assignment fails, delete the user
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Failed to assign Admin role: {errors}" });
            }

            return Ok(new
            {
                message = "Admin user created successfully",
                userId = user.Id,
                userName = user.UserName,
                email = user.Email
            });
        }

        public sealed record UserAdminDto(
            Guid Id,
            string UserName,
            string Email,
            string? FullName,
            bool EmailConfirmed,
            DateTime? CreatedAt,
            DateTime? DateOfBirth,
            int? HeightCm,
            decimal? Weight,
            string? Gender,
            string? BloodType,
            List<string> Roles,
            int PrescriptionCount,
            int MedicineCount,
            DateTimeOffset? LockoutEnd);

        public sealed record SetUserRolesRequest(List<string>? Roles);

        public sealed record LockoutRequest(bool LockoutEnabled);

        public sealed record CreateAdminRequest(
            string UserName,
            string Email,
            string Password,
            string? FirstName,
            string? LastName,
            bool? EmailConfirmed);

        public sealed record AuditLogDto(
            Guid Id,
            Guid UserId,
            string UserName,
            string Action,
            string EntityType,
            Guid? EntityId,
            string? RequestPath,
            string? RequestMethod,
            string? ResponseStatus,
            string? IpAddress,
            DateTime CreatedAt,
            string? AdditionalData);
    }
}


