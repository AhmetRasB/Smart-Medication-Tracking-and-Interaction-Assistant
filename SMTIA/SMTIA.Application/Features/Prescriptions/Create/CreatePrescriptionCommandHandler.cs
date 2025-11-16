using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.Create
{
    internal sealed class CreatePrescriptionCommandHandler(
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository,
        IRepository<Medicine> medicineRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePrescriptionCommand, Result<CreatePrescriptionCommandResponse>>
    {
        public async Task<Result<CreatePrescriptionCommandResponse>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
        {
            // Validate that all medicines exist
            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);
            var medicineIds = request.Medicines.Select(m => m.MedicineId).ToList();
            var existingMedicines = allMedicines
                .Where(m => medicineIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToList();

            var missingMedicines = medicineIds.Except(existingMedicines).ToList();
            if (missingMedicines.Any())
            {
                return (404, $"Bulunamayan ilaç ID'leri: {string.Join(", ", missingMedicines)}");
            }

            // Convert dates to UTC
            var prescriptionDate = request.PrescriptionDate;
            if (prescriptionDate.Kind == DateTimeKind.Unspecified)
            {
                prescriptionDate = DateTime.SpecifyKind(prescriptionDate, DateTimeKind.Utc);
            }
            else
            {
                prescriptionDate = prescriptionDate.ToUniversalTime();
            }

            var startDate = request.StartDate;
            if (startDate.Kind == DateTimeKind.Unspecified)
            {
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            }
            else
            {
                startDate = startDate.ToUniversalTime();
            }

            DateTime? endDateUtc = null;
            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value;
                if (endDate.Kind == DateTimeKind.Unspecified)
                {
                    endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                }
                else
                {
                    endDateUtc = endDate.ToUniversalTime();
                }
            }

            // Create prescription
            var prescription = new UserPrescription
            {
                UserId = request.UserId,
                DoctorName = request.DoctorName,
                DoctorSpecialty = request.DoctorSpecialty,
                PrescriptionNumber = request.PrescriptionNumber,
                PrescriptionDate = prescriptionDate,
                StartDate = startDate,
                EndDate = endDateUtc,
                Notes = request.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await prescriptionRepository.AddAsync(prescription, cancellationToken);

            // Add prescription medicines
            foreach (var medicineDto in request.Medicines)
            {
                var prescriptionMedicine = new PrescriptionMedicine
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = medicineDto.MedicineId,
                    Dosage = medicineDto.Dosage,
                    DosageUnit = medicineDto.DosageUnit,
                    Quantity = medicineDto.Quantity,
                    Instructions = medicineDto.Instructions,
                    CreatedAt = DateTime.UtcNow
                };

                await prescriptionMedicineRepository.AddAsync(prescriptionMedicine, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreatePrescriptionCommandResponse(prescription.Id, "Reçete başarıyla oluşturuldu");
        }
    }
}

