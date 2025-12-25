using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class UserSurgery : Entity
    {
        public Guid UserId { get; set; }
        public string SurgeryName { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public AppUser User { get; set; } = null!;
    }
}
