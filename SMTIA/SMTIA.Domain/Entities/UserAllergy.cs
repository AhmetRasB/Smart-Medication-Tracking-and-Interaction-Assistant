using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class UserAllergy : Entity
    {
        public Guid UserId { get; set; }
        public string AllergyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Severity { get; set; } // Hafif, Orta, Şiddetli
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public AppUser User { get; set; } = null!;
    }
}

