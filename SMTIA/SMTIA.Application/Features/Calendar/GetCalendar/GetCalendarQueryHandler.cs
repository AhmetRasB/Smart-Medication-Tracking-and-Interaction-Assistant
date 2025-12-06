using MediatR;
using Microsoft.EntityFrameworkCore;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Calendar.GetCalendar
{
    internal sealed class GetCalendarQueryHandler(
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<ScheduleTiming> timingRepository,
        IRepository<IntakeLog> intakeLogRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository,
        IRepository<Medicine> medicineRepository) : IRequestHandler<GetCalendarQuery, Result<GetCalendarQueryResponse>>
    {
        public async Task<Result<GetCalendarQueryResponse>> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
        {
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

            var endDate = request.EndDate;
            if (endDate.Kind == DateTimeKind.Unspecified)
            {
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            }
            else
            {
                endDate = endDate.ToUniversalTime();
            }

            // Get all user prescriptions
            var allPrescriptions = await prescriptionRepository.ListAllAsync(cancellationToken);
            var userPrescriptionIds = allPrescriptions
                .Where(p => p.UserId == request.UserId)
                .Select(p => p.Id)
                .ToList();

            // Get all active schedules for user prescriptions
            var allSchedules = await scheduleRepository.ListAllAsync(cancellationToken);
            var userSchedules = allSchedules
                .Where(s => userPrescriptionIds.Contains(s.PrescriptionId) && 
                           s.IsActive && 
                           !s.IsDeleted &&
                           (s.EndDate == null || s.EndDate >= startDate) &&
                           s.StartDate <= endDate)
                .ToList();

            // Get all timings for these schedules
            var allTimings = await timingRepository.ListAllAsync(cancellationToken);
            var scheduleTimings = allTimings
                .Where(t => userSchedules.Select(s => s.Id).Contains(t.MedicationScheduleId) &&
                           t.IsActive &&
                           !t.IsDeleted)
                .ToList();

            // Get all intake logs for these schedules in the date range
            var allIntakeLogs = await intakeLogRepository.ListAllAsync(cancellationToken);
            var intakeLogs = allIntakeLogs
                .Where(l => userSchedules.Select(s => s.Id).Contains(l.MedicationScheduleId) &&
                           l.ScheduledTime >= startDate &&
                           l.ScheduledTime <= endDate &&
                           !l.IsDeleted)
                .ToList();

            // Get prescription medicines and medicines for medicine names
            var allPrescriptionMedicines = await prescriptionMedicineRepository.ListAllAsync(cancellationToken);
            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);

            var calendarItems = new List<CalendarItemDto>();

            // Generate calendar items from schedules and timings
            foreach (var schedule in userSchedules)
            {
                var timings = scheduleTimings.Where(t => t.MedicationScheduleId == schedule.Id).ToList();
                var prescriptionMedicine = allPrescriptionMedicines.FirstOrDefault(pm => pm.Id == schedule.PrescriptionMedicineId);
                var medicine = prescriptionMedicine != null 
                    ? allMedicines.FirstOrDefault(m => m.Id == prescriptionMedicine.MedicineId)
                    : null;
                var medicineName = medicine?.Name ?? "Bilinmeyen İlaç";

                foreach (var timing in timings)
                {
                    // Generate dates for this timing within the date range
                    var dates = GenerateDatesForTiming(schedule, timing, startDate, endDate);

                    foreach (var date in dates)
                    {
                        // Check if there's an existing intake log for this scheduled time
                        var existingLog = intakeLogs.FirstOrDefault(l => 
                            l.MedicationScheduleId == schedule.Id &&
                            l.ScheduledTime.Date == date.Date &&
                            l.ScheduledTime.TimeOfDay == timing.Time.ToTimeSpan());

                        if (existingLog != null)
                        {
                            // Use existing log
                            calendarItems.Add(new CalendarItemDto(
                                existingLog.Id,
                                schedule.Id,
                                schedule.PrescriptionId,
                                schedule.PrescriptionMedicineId,
                                schedule.ScheduleName,
                                medicineName,
                                existingLog.ScheduledTime,
                                timing.Dosage,
                                timing.DosageUnit,
                                existingLog.IsTaken,
                                existingLog.IsSkipped,
                                existingLog.TakenTime,
                                existingLog.Notes));
                        }
                        else
                        {
                            // Create a new calendar item (not yet logged)
                            var scheduledDateTime = date.Date.Add(timing.Time.ToTimeSpan());
                            calendarItems.Add(new CalendarItemDto(
                                Guid.Empty, // No log ID yet
                                schedule.Id,
                                schedule.PrescriptionId,
                                schedule.PrescriptionMedicineId,
                                schedule.ScheduleName,
                                medicineName,
                                scheduledDateTime,
                                timing.Dosage,
                                timing.DosageUnit,
                                false,
                                false,
                                null,
                                null));
                        }
                    }
                }
            }

            // Sort by scheduled time
            calendarItems = calendarItems.OrderBy(i => i.ScheduledTime).ToList();

            return new GetCalendarQueryResponse(calendarItems);
        }

        private List<DateTime> GenerateDatesForTiming(
            MedicationSchedule schedule,
            ScheduleTiming timing,
            DateTime startDate,
            DateTime endDate)
        {
            var dates = new List<DateTime>();
            var scheduleStartDate = schedule.StartDate.Date;
            var queryStartDate = startDate.Date > scheduleStartDate ? startDate.Date : scheduleStartDate;
            var effectiveEndDate = schedule.EndDate?.Date ?? endDate.Date;
            if (effectiveEndDate > endDate.Date)
            {
                effectiveEndDate = endDate.Date;
            }

            if (timing.IntervalHours.HasValue)
            {
                // Interval timing - every X hours
                var intervalHours = timing.IntervalHours.Value;
                var startDateTime = schedule.StartDate;
                
                // Find the first occurrence on or after query start date
                var currentDateTime = queryStartDate.Add(timing.Time.ToTimeSpan());
                
                // If currentDateTime is before query start date, move to next interval
                if (currentDateTime < startDate)
                {
                    var hoursToAdd = (int)Math.Ceiling((startDate - currentDateTime).TotalHours / intervalHours) * intervalHours;
                    currentDateTime = currentDateTime.AddHours(hoursToAdd);
                }
                
                // Ensure we start from schedule start date or later
                if (currentDateTime < startDateTime)
                {
                    var hoursToAdd = (int)Math.Ceiling((startDateTime - currentDateTime).TotalHours / intervalHours) * intervalHours;
                    currentDateTime = currentDateTime.AddHours(hoursToAdd);
                }

                // Generate all occurrences
                while (currentDateTime <= endDate && currentDateTime <= (schedule.EndDate ?? DateTime.MaxValue))
                {
                    dates.Add(currentDateTime);
                    currentDateTime = currentDateTime.AddHours(intervalHours);
                }
            }
            else if (timing.DayOfWeek.HasValue)
            {
                // Weekly timing - specific day of week
                // C# DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
                // ScheduleTiming.DayOfWeek: 0=Pazar, 1=Pazartesi, ..., 6=Cumartesi
                // They match, so we can use directly
                var targetDayOfWeek = timing.DayOfWeek.Value;
                
                // Start from query start date or schedule start date, whichever is later
                var currentDate = queryStartDate;
                
                // Find the first occurrence of the target day on or after currentDate
                var currentDayOfWeek = (int)currentDate.DayOfWeek;
                var daysToAdd = (targetDayOfWeek - currentDayOfWeek + 7) % 7;
                
                // If we're on the target day but before schedule start, move to next week
                if (daysToAdd == 0 && currentDate < scheduleStartDate)
                {
                    daysToAdd = 7;
                }
                
                currentDate = currentDate.AddDays(daysToAdd);

                // Generate all occurrences
                while (currentDate <= effectiveEndDate)
                {
                    // Only add if on or after schedule start date
                    if (currentDate >= scheduleStartDate)
                    {
                        dates.Add(currentDate);
                    }
                    currentDate = currentDate.AddDays(7); // Next week
                }
            }
            else
            {
                // Daily timing - every day
                var currentDate = queryStartDate;
                while (currentDate <= effectiveEndDate)
                {
                    dates.Add(currentDate);
                    currentDate = currentDate.AddDays(1);
                }
            }

            return dates;
        }
    }
}

