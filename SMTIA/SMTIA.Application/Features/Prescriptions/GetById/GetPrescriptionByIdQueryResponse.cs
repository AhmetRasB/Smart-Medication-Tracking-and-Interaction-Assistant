namespace SMTIA.Application.Features.Prescriptions.GetById
{
    public sealed record GetPrescriptionByIdQueryResponse(
        Guid Id,
        Guid UserId,
        string? DoctorName,
        string? DoctorSpecialty,
        string? PrescriptionNumber,
        DateTime PrescriptionDate,
        DateTime StartDate,
        DateTime? EndDate,
        string? Notes,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<PrescriptionMedicineResponse> Medicines);

    public sealed record PrescriptionMedicineResponse(
        Guid Id,
        Guid MedicineId,
        string MedicineName,
        decimal Dosage,
        string DosageUnit,
        int Quantity,
        string? Instructions);
}

