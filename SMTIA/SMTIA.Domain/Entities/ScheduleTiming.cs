using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class ScheduleTiming : Entity
    {
        public Guid MedicationScheduleId { get; set; }
        public TimeOnly Time { get; set; }
        public decimal Dosage { get; set; } 
        public string DosageUnit { get; set; } = string.Empty;
        public int? DayOfWeek { get; set; } 
        /// <summary>
        /// Interval tipi için saat aralığı (örn: 12 = her 12 saatte bir)
        /// </summary>
        public int? IntervalHours { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public MedicationSchedule MedicationSchedule { get; set; } = null!;
    }
}

