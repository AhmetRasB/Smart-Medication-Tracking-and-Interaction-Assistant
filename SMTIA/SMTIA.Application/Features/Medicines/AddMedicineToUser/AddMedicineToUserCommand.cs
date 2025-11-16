using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.AddMedicineToUser
{
    public sealed record AddMedicineToUserCommand(
        Guid UserId,
        string MedicineName,
        decimal Dosage,
        string DosageUnit, // mg, gr, ml, vb.
        int PackageSize, // Paket boyutu (kaç tablet/kapsül)
        int DailyDoseCount, // Günde kaç doz
        string? DoctorNote) : IRequest<Result<AddMedicineToUserCommandResponse>>;
}

