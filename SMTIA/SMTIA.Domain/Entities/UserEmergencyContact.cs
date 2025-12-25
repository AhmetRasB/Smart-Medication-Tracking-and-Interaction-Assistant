using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class UserEmergencyContact : Entity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public AppUser User { get; set; } = null!;
    }
}
