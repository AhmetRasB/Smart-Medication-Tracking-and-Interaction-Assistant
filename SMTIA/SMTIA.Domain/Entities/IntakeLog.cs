using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class IntakeLog : Entity
    {
        public Guid MedicationScheduleId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? TakenTime { get; set; }
        public bool IsTaken { get; set; } = false;
        public bool IsSkipped { get; set; } = false;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MedicationSchedule MedicationSchedule { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }
}

