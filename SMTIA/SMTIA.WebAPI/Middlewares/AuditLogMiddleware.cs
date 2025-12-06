using System.Text;
using SMTIA.Application.Services;

namespace SMTIA.WebAPI.Middlewares
{
    public sealed class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            // Skip logging for health checks and swagger
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/health") || path.Contains("/swagger") || path.Contains("/api/auth/confirm-email") || path.Contains("/api/auth/reset-password"))
            {
                await _next(context);
                return;
            }

            var userId = GetUserId(context);
            if (userId == null)
            {
                await _next(context);
                return;
            }

            var requestMethod = context.Request.Method;
            var requestPath = context.Request.Path.Value;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Read request body
            string? requestBody = null;
            if (context.Request.ContentLength > 0 && context.Request.ContentType?.Contains("application/json") == true)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // Capture response status
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            var responseStatus = context.Response.StatusCode.ToString();

            // Log the audit
            var action = DetermineAction(requestMethod, requestPath);
            var entityType = DetermineEntityType(requestPath);

            await auditLogService.LogAsync(
                userId.Value,
                action,
                entityType,
                entityId: ExtractEntityId(requestPath),
                requestPath: requestPath,
                requestMethod: requestMethod,
                requestBody: requestBody,
                responseStatus: responseStatus,
                ipAddress: ipAddress,
                userAgent: userAgent,
                cancellationToken: context.RequestAborted);

            // Copy response back
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private static Guid? GetUserId(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst("Id") ?? 
                             context.User?.FindFirst("UserId") ?? 
                             context.User?.FindFirst("sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        private static string DetermineAction(string method, string? path)
        {
            return method.ToUpper() switch
            {
                "GET" => path?.Contains("/search") == true ? "SEARCH" : "GET",
                "POST" => "CREATE",
                "PUT" => "UPDATE",
                "DELETE" => "DELETE",
                _ => method.ToUpper()
            };
        }

        private static string DetermineEntityType(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "Unknown";

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            {
                return segments[1].Replace("-", ""); // medicines -> Medicines, intake-logs -> IntakeLogs
            }

            return "Unknown";
        }

        private static Guid? ExtractEntityId(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (Guid.TryParse(segment, out var id))
                {
                    return id;
                }
            }

            return null;
        }
    }
}

