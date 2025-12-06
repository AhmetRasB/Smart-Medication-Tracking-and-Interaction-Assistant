namespace SMTIA.Application.Features.AuditLogs.GetUserLogs
{
    public sealed record GetUserAuditLogsQueryResponse(
        List<AuditLogDto> Logs);

    public sealed record AuditLogDto(
        Guid Id,
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

