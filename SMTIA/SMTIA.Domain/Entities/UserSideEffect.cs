using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class UserSideEffect : Entity
    {
        public Guid UserId { get; set; }
        public Guid? MedicineId { get; set; } // Optional link to a medicine
        public string MedicineName { get; set; } = string.Empty; // Store name in case medicine is deleted or custom
        public string Severity { get; set; } = "mild"; // mild, moderate, severe, critical
        public string SideEffects { get; set; } = string.Empty; // JSON or comma separated list of effects
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public AppUser User { get; set; } = null!;
        public Medicine? Medicine { get; set; }
    }
}
