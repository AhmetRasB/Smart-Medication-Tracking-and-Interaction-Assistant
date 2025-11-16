using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.GetById
{
    internal sealed class GetPrescriptionByIdQueryHandler(
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository,
        IRepository<Medicine> medicineRepository) : IRequestHandler<GetPrescriptionByIdQuery, Result<GetPrescriptionByIdQueryResponse>>
    {
        public async Task<Result<GetPrescriptionByIdQueryResponse>> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
        {
            var prescription = await prescriptionRepository.GetByIdAsync(request.Id, cancellationToken);

            if (prescription == null)
            {
                return (404, "Reçete bulunamadı");
            }

            // Get all prescription medicines and medicines
            var allPrescriptionMedicines = await prescriptionMedicineRepository.ListAllAsync(cancellationToken);
            var prescriptionMedicines = allPrescriptionMedicines
                .Where(pm => pm.PrescriptionId == prescription.Id)
                .ToList();

            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);
            var medicines = prescriptionMedicines.Select(pm =>
            {
                var medicine = allMedicines.FirstOrDefault(m => m.Id == pm.MedicineId);
                return new PrescriptionMedicineResponse(
                    pm.Id,
                    pm.MedicineId,
                    medicine?.Name ?? "Bilinmeyen İlaç",
                    pm.Dosage,
                    pm.DosageUnit,
                    pm.Quantity,
                    pm.Instructions
                );
            }).ToList();

            return new GetPrescriptionByIdQueryResponse(
                prescription.Id,
                prescription.UserId,
                prescription.DoctorName,
                prescription.DoctorSpecialty,
                prescription.PrescriptionNumber,
                prescription.PrescriptionDate,
                prescription.StartDate,
                prescription.EndDate,
                prescription.Notes,
                prescription.IsActive,
                prescription.CreatedAt,
                prescription.UpdatedAt,
                medicines
            );
        }
    }
}

