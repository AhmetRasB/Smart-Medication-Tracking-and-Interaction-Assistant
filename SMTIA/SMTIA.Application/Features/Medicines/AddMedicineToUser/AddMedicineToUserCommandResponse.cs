namespace SMTIA.Application.Features.Medicines.AddMedicineToUser
{
    public sealed record AddMedicineToUserCommandResponse(
        Guid MedicineId,
        string Message);
}

