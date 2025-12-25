using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecurityAlgorithms = Microsoft.IdentityModel.Tokens.SecurityAlgorithms;

namespace SMTIA.Infrastructure.Options
{
    public sealed class JwtTokenOptionsSetup(
        IOptions<JwtOptions> jwtOptions) : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.ValidIssuer = jwtOptions.Value.Issuer;
            options.TokenValidationParameters.ValidAudience = jwtOptions.Value.Audience;
            // Allow 5 minutes clock skew for token validation
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));
            options.TokenValidationParameters.IssuerSigningKey = securityKey;
            // Explicitly set the algorithm to match JwtProvider (HmacSha512)
            options.TokenValidationParameters.AlgorithmValidator = (algorithm, securityKey, securityToken, validationParameters) =>
            {
                // Accept HmacSha512 (used in JwtProvider)
                if (algorithm == SecurityAlgorithms.HmacSha512)
                    return true;
                return false;
            };

            // Configure JWT Bearer Events for logging (only if not already set)
            if (options.Events == null)
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        var tokenPreview = authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader;
                        logger.LogError(context.Exception, "JWT Authentication failed: {Error}. Token preview: {TokenPreview}", context.Exception.Message, tokenPreview);
                        
                        // Log detailed error information
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            logger.LogError("Token has expired");
                        }
                        else if (context.Exception is SecurityTokenInvalidSignatureException)
                        {
                            logger.LogError("Token signature is invalid");
                        }
                        else if (context.Exception is SecurityTokenInvalidIssuerException)
                        {
                            logger.LogError("Token issuer is invalid. Expected: {ExpectedIssuer}, Got: {ActualIssuer}", jwtOptions.Value.Issuer, context.Exception.Message);
                        }
                        else if (context.Exception is SecurityTokenInvalidAudienceException)
                        {
                            logger.LogError("Token audience is invalid. Expected: {ExpectedAudience}", jwtOptions.Value.Audience);
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        var userId = context.Principal?.FindFirst("Id")?.Value;
                        var email = context.Principal?.FindFirst("Email")?.Value;
                        logger.LogInformation("JWT Token validated successfully for user: {UserId} ({Email})", userId, email);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        logger.LogWarning("JWT Challenge: {Error}, {ErrorDescription}. Authorization Header present: {HasAuthHeader}", 
                            context.Error, context.ErrorDescription, !string.IsNullOrEmpty(authHeader));
                        return Task.CompletedTask;
                    }
                };
            }
        }
    }
}