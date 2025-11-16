namespace SMTIA.Application.Features.Prescriptions.GetUserPrescriptions
{
    public sealed record GetUserPrescriptionsQueryResponse(
        Guid Id,
        string? DoctorName,
        string? DoctorSpecialty,
        string? PrescriptionNumber,
        DateTime PrescriptionDate,
        DateTime StartDate,
        DateTime? EndDate,
        bool IsActive,
        int MedicineCount);
}

