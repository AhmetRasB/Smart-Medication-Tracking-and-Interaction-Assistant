namespace SMTIA.Application.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(
            Guid userId,
            string action,
            string entityType,
            Guid? entityId = null,
            string? requestPath = null,
            string? requestMethod = null,
            string? requestBody = null,
            string? responseStatus = null,
            string? ipAddress = null,
            string? userAgent = null,
            string? additionalData = null,
            CancellationToken cancellationToken = default);
    }
}

