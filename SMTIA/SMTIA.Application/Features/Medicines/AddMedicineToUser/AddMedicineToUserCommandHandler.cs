using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.AddMedicineToUser
{
    internal sealed class AddMedicineToUserCommandHandler(
        IRepository<Medicine> medicineRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<AddMedicineToUserCommand, Result<AddMedicineToUserCommandResponse>>
    {
        public async Task<Result<AddMedicineToUserCommandResponse>> Handle(AddMedicineToUserCommand request, CancellationToken cancellationToken)
        {
            // Check if medicine exists, if not create it
            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);
            var existingMedicine = allMedicines.FirstOrDefault(m => 
                m.Name.Equals(request.MedicineName, StringComparison.OrdinalIgnoreCase));

            Medicine medicine;
            if (existingMedicine == null)
            {
                // Create new medicine
                medicine = new Medicine
                {
                    Name = request.MedicineName,
                    CreatedAt = DateTime.UtcNow
                };
                await medicineRepository.AddAsync(medicine, cancellationToken);
            }
            else
            {
                medicine = existingMedicine;
            }

            // Create a prescription for the user
            var prescription = new UserPrescription
            {
                UserId = request.UserId,
                PrescriptionDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                Notes = request.DoctorNote,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await prescriptionRepository.AddAsync(prescription, cancellationToken);

            // Add medicine to prescription
            var prescriptionMedicine = new PrescriptionMedicine
            {
                PrescriptionId = prescription.Id,
                MedicineId = medicine.Id,
                Dosage = request.Dosage,
                DosageUnit = request.DosageUnit,
                Quantity = request.PackageSize,
                Instructions = $"Günde {request.DailyDoseCount} doz",
                CreatedAt = DateTime.UtcNow
            };
            await prescriptionMedicineRepository.AddAsync(prescriptionMedicine, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddMedicineToUserCommandResponse(medicine.Id, "İlaç başarıyla eklendi");
        }
    }
}

