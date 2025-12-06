using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.GetUserSchedules
{
    internal sealed class GetUserSchedulesQueryHandler(
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<ScheduleTiming> timingRepository) : IRequestHandler<GetUserSchedulesQuery, Result<GetUserSchedulesQueryResponse>>
    {
        public async Task<Result<GetUserSchedulesQueryResponse>> Handle(GetUserSchedulesQuery request, CancellationToken cancellationToken)
        {
            // Get all user prescriptions
            var allPrescriptions = await prescriptionRepository.ListAllAsync(cancellationToken);
            var userPrescriptionIds = allPrescriptions
                .Where(p => p.UserId == request.UserId)
                .Select(p => p.Id)
                .ToList();

            // Get all schedules
            var allSchedules = await scheduleRepository.ListAllAsync(cancellationToken);
            var userSchedules = allSchedules
                .Where(s => userPrescriptionIds.Contains(s.PrescriptionId))
                .ToList();

            // Get all timings
            var allTimings = await timingRepository.ListAllAsync(cancellationToken);

            var schedules = userSchedules.Select(s =>
            {
                var timingCount = allTimings.Count(t => t.MedicationScheduleId == s.Id);
                return new ScheduleDto(
                    s.Id,
                    s.PrescriptionId,
                    s.PrescriptionMedicineId,
                    s.ScheduleName,
                    s.StartDate,
                    s.EndDate,
                    s.IsActive,
                    timingCount);
            }).ToList();

            return new GetUserSchedulesQueryResponse(schedules);
        }
    }
}

