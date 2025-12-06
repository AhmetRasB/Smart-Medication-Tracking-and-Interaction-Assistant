namespace SMTIA.Application.Features.IntakeLogs.Create
{
    public sealed record CreateIntakeLogCommandResponse(
        Guid Id,
        string Message);
}

