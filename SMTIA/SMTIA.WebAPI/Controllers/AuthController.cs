using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using SMTIA.Application.Features.Auth.ConfirmEmail;
using SMTIA.Application.Features.Auth.ForgotPassword;
using SMTIA.Application.Features.Auth.Login;
using SMTIA.Application.Features.Auth.Register;
using SMTIA.Application.Features.Auth.ResetPassword;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [AllowAnonymous]
    public sealed class AuthController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtProvider _jwtProvider;
        private readonly IConfiguration _configuration;

        public AuthController(IMediator mediator, UserManager<AppUser> userManager, IJwtProvider jwtProvider, IConfiguration configuration) : base(mediator)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token, CancellationToken cancellationToken)
        {
            var command = new ConfirmEmailCommand(email, token);
            var response = await _mediator.Send(command, cancellationToken);
            
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            
            if (response.StatusCode == 200)
            {
                // Email confirmation basarili, otomatik login i�in token olustur
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && user.EmailConfirmed)
                {
                    var loginResponse = await _jwtProvider.CreateToken(user);
                    // Frontend'e redirect et, token query parameter olarak g�nder
                    var redirectUrl = $"{frontendUrl}/email-confirmation?success=true&token={Uri.EscapeDataString(loginResponse.Token)}&email={Uri.EscapeDataString(email)}";
                    return Redirect(redirectUrl);
                }
                // Token olusturulamadi ama onay basarili
                var redirectUrlSuccess = $"{frontendUrl}/email-confirmation?success=true&email={Uri.EscapeDataString(email)}";
                return Redirect(redirectUrlSuccess);
            }
            else if (response.StatusCode == 404)
            {
                var redirectUrlError = $"{frontendUrl}/email-confirmation?success=false&error=Kullanici bulunamadi";
                return Redirect(redirectUrlError);
            }
            else if (response.StatusCode == 400)
            {
                var errorMessage = response.Data?.ToString() ?? "E-posta onayi basarisiz oldu. L�tfen ge�erli bir onay linki kullanin.";
                var redirectUrlError = $"{frontendUrl}/email-confirmation?success=false&error={Uri.EscapeDataString(errorMessage)}";
                return Redirect(redirectUrlError);
            }
            
            var redirectUrlUnknown = $"{frontendUrl}/email-confirmation?success=false&error=Beklenmeyen bir hata olustu";
            return Redirect(redirectUrlUnknown);
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPasswordPage([FromQuery] string email, [FromQuery] string token)
        {
            // Frontend'e redirect et
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var redirectUrl = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            return Redirect(redirectUrl);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            
            if (response.StatusCode == 200)
            {
                // Sifre sifirlama basarili, otomatik login i�in token olustur
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    var loginResponse = await _jwtProvider.CreateToken(user);
                    return Ok(new
                    {
                        success = true,
                        token = loginResponse.Token,
                        email = request.Email,
                        message = "Sifreniz basariyla sifirlandi. Otomatik olarak giris yapiliyor..."
                    });
                }
                return Ok(new
                {
                    success = true,
                    email = request.Email,
                    message = "Sifreniz basariyla sifirlandi."
                });
            }
            
            // Hata durumunda JSON response d�nd�r
            var errorMessage = response.Data is ResetPasswordCommandResponse responseData 
                ? responseData.Message 
                : "Sifre sifirlama basarisiz oldu. L�tfen ge�erli bir link kullanin veya yeni bir sifre sifirlama talebinde bulunun.";
            
            return StatusCode(response.StatusCode, new
            {
                success = false,
                message = errorMessage
            });
        }
    }
}
