using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.GetUserPrescriptions
{
    internal sealed class GetUserPrescriptionsQueryHandler(
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository) : IRequestHandler<GetUserPrescriptionsQuery, Result<List<GetUserPrescriptionsQueryResponse>>>
    {
        public async Task<Result<List<GetUserPrescriptionsQueryResponse>>> Handle(GetUserPrescriptionsQuery request, CancellationToken cancellationToken)
        {
            var allPrescriptions = await prescriptionRepository.ListAllAsync(cancellationToken);
            var userPrescriptions = allPrescriptions
                .Where(p => p.UserId == request.UserId)
                .ToList();

            var allPrescriptionMedicines = await prescriptionMedicineRepository.ListAllAsync(cancellationToken);

            var result = userPrescriptions.Select(p =>
            {
                var medicineCount = allPrescriptionMedicines.Count(pm => pm.PrescriptionId == p.Id);
                return new GetUserPrescriptionsQueryResponse(
                    p.Id,
                    p.DoctorName,
                    p.DoctorSpecialty,
                    p.PrescriptionNumber,
                    p.PrescriptionDate,
                    p.StartDate,
                    p.EndDate,
                    p.IsActive,
                    medicineCount
                );
            }).ToList();

            return result;
        }
    }
}

