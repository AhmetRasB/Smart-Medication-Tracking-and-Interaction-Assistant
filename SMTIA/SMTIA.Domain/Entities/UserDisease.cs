using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class UserDisease : Entity
    {
        public Guid UserId { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public AppUser User { get; set; } = null!;
    }
}

