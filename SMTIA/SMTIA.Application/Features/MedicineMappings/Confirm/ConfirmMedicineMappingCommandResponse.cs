namespace SMTIA.Application.Features.MedicineMappings.Confirm
{
    public sealed record ConfirmMedicineMappingCommandResponse(
        Guid MappingId,
        string Message);
}


