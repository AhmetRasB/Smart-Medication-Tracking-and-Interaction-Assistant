using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SMTIA.Application.Features.Auth.Login;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Options;

namespace SMTIA.Infrastructure.Services
{
    internal class JwtProvider(
        UserManager<AppUser> userManager,
        IOptions<JwtOptions> jwtOptions) : IJwtProvider
    {
        public async Task<LoginCommandResponse> CreateToken(AppUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            
            List<Claim> claims = new()
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Name", user.FullName),
                new Claim("Email", user.Email ?? ""),
                new Claim("UserName", user.UserName ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            DateTime expires = DateTime.UtcNow.AddHours(24); // Token 24 saat geçerli

            // Validate JWT options
            if (string.IsNullOrWhiteSpace(jwtOptions.Value.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }
            if (string.IsNullOrWhiteSpace(jwtOptions.Value.Issuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured");
            }
            if (string.IsNullOrWhiteSpace(jwtOptions.Value.Audience))
            {
                throw new InvalidOperationException("JWT Audience is not configured");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));

            JwtSecurityToken jwtSecurityToken = new(
                issuer: jwtOptions.Value.Issuer,
                audience: jwtOptions.Value.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512));

            JwtSecurityTokenHandler handler = new();

            string token = handler.WriteToken(jwtSecurityToken);

            string refreshToken = Guid.NewGuid().ToString();
            DateTime refreshTokenExpires = expires.AddHours(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpires = refreshTokenExpires;

            await userManager.UpdateAsync(user);

            return new(token, refreshToken, refreshTokenExpires);
        }
    }
}
