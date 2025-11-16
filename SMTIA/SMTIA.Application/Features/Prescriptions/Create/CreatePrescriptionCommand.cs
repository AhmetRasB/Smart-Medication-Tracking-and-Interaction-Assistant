using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.Create
{
    public sealed record CreatePrescriptionCommand(
        Guid UserId,
        string? DoctorName,
        string? DoctorSpecialty,
        string? PrescriptionNumber,
        DateTime PrescriptionDate,
        DateTime StartDate,
        DateTime? EndDate,
        string? Notes,
        List<PrescriptionMedicineDto> Medicines) : IRequest<Result<CreatePrescriptionCommandResponse>>;

    public sealed record PrescriptionMedicineDto(
        Guid MedicineId,
        decimal Dosage,
        string DosageUnit,
        int Quantity,
        string? Instructions);
}

