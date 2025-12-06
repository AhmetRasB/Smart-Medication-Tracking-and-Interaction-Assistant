using Microsoft.Extensions.Logging;
using SMTIA.Application.Abstractions;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Services
{
    internal sealed class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            IRepository<AuditLog> auditLogRepository,
            IUnitOfWork unitOfWork,
            ILogger<AuditLogService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task LogAsync(
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
            CancellationToken cancellationToken = default)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    RequestPath = requestPath,
                    RequestMethod = requestMethod,
                    RequestBody = requestBody,
                    ResponseStatus = responseStatus,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    CreatedAt = DateTime.UtcNow
                };

                await _auditLogRepository.AddAsync(auditLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Serilog ile de logla
                _logger.LogInformation(
                    "Audit Log: UserId={UserId}, Action={Action}, EntityType={EntityType}, EntityId={EntityId}, Path={Path}, Method={Method}, Status={Status}, IP={IP}",
                    userId, action, entityType, entityId, requestPath, requestMethod, responseStatus, ipAddress);
            }
            catch (Exception ex)
            {
                // Audit log hatası sistemin çalışmasını engellememeli
                _logger.LogError(ex, "Error creating audit log for UserId={UserId}, Action={Action}", userId, action);
            }
        }
    }
}

