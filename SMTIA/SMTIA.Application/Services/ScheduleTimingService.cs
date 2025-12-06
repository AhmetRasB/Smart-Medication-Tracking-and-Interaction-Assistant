using SMTIA.Domain.Entities;

namespace SMTIA.Application.Services
{
    internal sealed class ScheduleTimingService : IScheduleTimingService
    {
        public List<ScheduleTiming> GenerateTimingsFromRule(
            MedicationSchedule schedule,
            ScheduleTimingRule rule,
            decimal dosage,
            string dosageUnit)
        {
            var timings = new List<ScheduleTiming>();

            switch (rule.Type)
            {
                case ScheduleTimingType.Interval:
                    timings = GenerateIntervalTimings(schedule, rule, dosage, dosageUnit);
                    break;

                case ScheduleTimingType.Weekly:
                    timings = GenerateWeeklyTimings(schedule, rule, dosage, dosageUnit);
                    break;

                case ScheduleTimingType.Daily:
                    timings = GenerateDailyTimings(schedule, rule, dosage, dosageUnit);
                    break;

                default:
                    throw new ArgumentException($"Desteklenmeyen zamanlama tipi: {rule.Type}");
            }

            return timings;
        }

        private List<ScheduleTiming> GenerateIntervalTimings(
            MedicationSchedule schedule,
            ScheduleTimingRule rule,
            decimal dosage,
            string dosageUnit)
        {
            if (!rule.IntervalHours.HasValue || rule.IntervalHours.Value <= 0)
            {
                throw new ArgumentException("Interval tipi için IntervalHours değeri gerekli ve 0'dan büyük olmalıdır");
            }

            // Interval tipinde sadece bir ScheduleTiming oluştur
            // Calendar endpoint'inde bu ScheduleTiming'e göre tarihler generate edilecek
            var timing = new ScheduleTiming
            {
                MedicationScheduleId = schedule.Id,
                Time = TimeOnly.FromDateTime(schedule.StartDate),
                Dosage = dosage,
                DosageUnit = dosageUnit,
                DayOfWeek = null, // Interval tipinde gün belirtilmez
                IntervalHours = rule.IntervalHours.Value, // Interval bilgisini sakla
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return new List<ScheduleTiming> { timing };
        }

        private List<ScheduleTiming> GenerateWeeklyTimings(
            MedicationSchedule schedule,
            ScheduleTimingRule rule,
            decimal dosage,
            string dosageUnit)
        {
            if (rule.DaysOfWeek == null || !rule.DaysOfWeek.Any())
            {
                throw new ArgumentException("Weekly tipi için DaysOfWeek listesi gerekli");
            }

            if (!rule.Time.HasValue)
            {
                throw new ArgumentException("Weekly tipi için Time değeri gerekli");
            }

            var timings = new List<ScheduleTiming>();
            var endDate = schedule.EndDate ?? schedule.StartDate.AddYears(1);

            // Her belirtilen gün için bir timing oluştur
            foreach (var dayOfWeek in rule.DaysOfWeek)
            {
                if (dayOfWeek < 0 || dayOfWeek > 6)
                {
                    throw new ArgumentException($"Geçersiz gün değeri: {dayOfWeek}. 0-6 arası olmalıdır (0=Pazar, 6=Cumartesi)");
                }

                var timing = new ScheduleTiming
                {
                    MedicationScheduleId = schedule.Id,
                    Time = rule.Time.Value,
                    Dosage = dosage,
                    DosageUnit = dosageUnit,
                    DayOfWeek = dayOfWeek,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                timings.Add(timing);
            }

            return timings;
        }

        private List<ScheduleTiming> GenerateDailyTimings(
            MedicationSchedule schedule,
            ScheduleTimingRule rule,
            decimal dosage,
            string dosageUnit)
        {
            if (rule.DailyTimes == null || !rule.DailyTimes.Any())
            {
                throw new ArgumentException("Daily tipi için DailyTimes listesi gerekli");
            }

            var timings = new List<ScheduleTiming>();

            // Her belirtilen saat için bir timing oluştur
            foreach (var time in rule.DailyTimes)
            {
                var timing = new ScheduleTiming
                {
                    MedicationScheduleId = schedule.Id,
                    Time = time,
                    Dosage = dosage,
                    DosageUnit = dosageUnit,
                    DayOfWeek = null, // Daily tipinde gün belirtilmez (her gün)
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                timings.Add(timing);
            }

            return timings;
        }
    }
}

