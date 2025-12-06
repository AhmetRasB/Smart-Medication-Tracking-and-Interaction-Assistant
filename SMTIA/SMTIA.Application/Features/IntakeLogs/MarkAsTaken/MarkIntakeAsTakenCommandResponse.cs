namespace SMTIA.Application.Features.IntakeLogs.MarkAsTaken
{
    public sealed record MarkIntakeAsTakenCommandResponse(
        Guid Id,
        string Message);
}

