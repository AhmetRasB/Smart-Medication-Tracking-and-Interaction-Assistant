using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.Create
{
    internal sealed class CreateScheduleCommandHandler(
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<ScheduleTiming> timingRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository,
        IScheduleTimingService scheduleTimingService,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateScheduleCommand, Result<CreateScheduleCommandResponse>>
    {
        public async Task<Result<CreateScheduleCommandResponse>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
        {
            // Validate prescription exists and belongs to user
            var prescription = await prescriptionRepository.GetByIdAsync(request.PrescriptionId, cancellationToken);
            if (prescription == null || prescription.UserId != request.UserId)
            {
                return (404, "Reçete bulunamadı veya kullanıcıya ait değil");
            }

            // Validate prescription medicine exists
            var prescriptionMedicine = await prescriptionMedicineRepository.GetByIdAsync(request.PrescriptionMedicineId, cancellationToken);
            if (prescriptionMedicine == null || prescriptionMedicine.PrescriptionId != request.PrescriptionId)
            {
                return (404, "Reçete ilacı bulunamadı");
            }

            // Convert dates to UTC
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

            // Create schedule
            var schedule = new MedicationSchedule
            {
                PrescriptionId = request.PrescriptionId,
                PrescriptionMedicineId = request.PrescriptionMedicineId,
                ScheduleName = request.ScheduleName,
                StartDate = startDate,
                EndDate = endDateUtc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await scheduleRepository.AddAsync(schedule, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Convert DTO rule to service rule
            var timingRule = new ScheduleTimingRule
            {
                Type = (ScheduleTimingType)request.TimingRule.Type,
                IntervalHours = request.TimingRule.IntervalHours,
                DaysOfWeek = request.TimingRule.DaysOfWeek,
                Time = request.TimingRule.Time,
                DailyTimes = request.TimingRule.DailyTimes
            };

            // Generate timings from rule using ScheduleTimingService
            var timings = scheduleTimingService.GenerateTimingsFromRule(
                schedule,
                timingRule,
                request.Dosage,
                request.DosageUnit);

            // Save generated timings
            foreach (var timing in timings)
            {
                await timingRepository.AddAsync(timing, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateScheduleCommandResponse(schedule.Id, "Takvim başarıyla oluşturuldu");
        }
    }
}

