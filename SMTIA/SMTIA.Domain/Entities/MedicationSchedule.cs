using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class MedicationSchedule : Entity
    {
        public Guid PrescriptionId { get; set; }
        public Guid PrescriptionMedicineId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public UserPrescription Prescription { get; set; } = null!;
        public PrescriptionMedicine PrescriptionMedicine { get; set; } = null!;
        public ICollection<ScheduleTiming> ScheduleTimings { get; set; } = new List<ScheduleTiming>();
        public ICollection<IntakeLog> IntakeLogs { get; set; } = new List<IntakeLog>();
    }
}

