using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class ProfileController : ApiController
    {
        public ProfileController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var userId = User.FindFirst("Id")?.Value;
            var email = User.FindFirst("Email")?.Value;
            var name = User.FindFirst("Name")?.Value;
            var userName = User.FindFirst("UserName")?.Value;

            var profileData = new
            {
                Id = userId,
                Email = email,
                Name = name,
                UserName = userName
            };

            return Ok(profileData);
        }
    }
}
