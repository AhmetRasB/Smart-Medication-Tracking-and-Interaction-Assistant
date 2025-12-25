using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMTIA.Domain.Entities;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public sealed class AdminController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AdminController(
            MediatR.IMediator mediator,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager) : base(mediator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            // NOTE: Identity IQueryable doesn't support CancellationToken in older APIs; keep it simple.
            var users = _userManager.Users.ToList();

            var result = new List<UserAdminDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserAdminDto(
                    u.Id,
                    u.UserName ?? "",
                    u.Email ?? "",
                    u.EmailConfirmed,
                    roles.ToList()));
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

        public sealed record UserAdminDto(
            Guid Id,
            string UserName,
            string Email,
            bool EmailConfirmed,
            List<string> Roles);

        public sealed record SetUserRolesRequest(List<string>? Roles);
    }
}


