using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class AuditLog : Entity
    {
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, GET, SEARCH, etc.
        public string EntityType { get; set; } = string.Empty; // Medicine, Prescription, Schedule, etc.
        public Guid? EntityId { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseStatus { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? AdditionalData { get; set; } // JSON formatında ekstra bilgiler

        // Navigation property
        public AppUser User { get; set; } = null!;
    }
}

