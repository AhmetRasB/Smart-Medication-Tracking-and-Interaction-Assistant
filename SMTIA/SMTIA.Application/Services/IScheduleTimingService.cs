using SMTIA.Domain.Entities;

namespace SMTIA.Application.Services
{
    public interface IScheduleTimingService
    {
        /// <summary>
        /// Kullanıcıdan gelen zamanlama kurallarını somut ScheduleTiming kayıtlarına dönüştürür.
        /// </summary>
        /// <param name="schedule">İlaç takvimi</param>
        /// <param name="rule">Zamanlama kuralı (örn: "her 12 saatte bir", "haftada 3 gün (Pzt, Çar, Cuma) sabah 9'da")</param>
        /// <param name="dosage">Doza miktarı</param>
        /// <param name="dosageUnit">Doza birimi</param>
        /// <returns>Oluşturulacak ScheduleTiming kayıtları</returns>
        List<ScheduleTiming> GenerateTimingsFromRule(
            MedicationSchedule schedule,
            ScheduleTimingRule rule,
            decimal dosage,
            string dosageUnit);
    }

    /// <summary>
    /// Zamanlama kuralı temsil eder
    /// </summary>
    public sealed class ScheduleTimingRule
    {
        /// <summary>
        /// Zamanlama tipi
        /// </summary>
        public ScheduleTimingType Type { get; set; }

        /// <summary>
        /// Interval (saat cinsinden) - Type = Interval ise kullanılır
        /// </summary>
        public int? IntervalHours { get; set; }

        /// <summary>
        /// Günler (0=Pazar, 1=Pazartesi, ..., 6=Cumartesi) - Type = Weekly ise kullanılır
        /// </summary>
        public List<int>? DaysOfWeek { get; set; }

        /// <summary>
        /// Saat - Type = Weekly ise kullanılır
        /// </summary>
        public TimeOnly? Time { get; set; }

        /// <summary>
        /// Günlük saatler - Type = Daily ise kullanılır
        /// </summary>
        public List<TimeOnly>? DailyTimes { get; set; }
    }

    public enum ScheduleTimingType
    {
        /// <summary>
        /// Belirli aralıklarla (örn: her 12 saatte bir)
        /// </summary>
        Interval,

        /// <summary>
        /// Haftalık belirli günlerde (örn: haftada 3 gün sabah 9'da)
        /// </summary>
        Weekly,

        /// <summary>
        /// Günlük belirli saatlerde (örn: sabah 9, akşam 21)
        /// </summary>
        Daily
    }
}

