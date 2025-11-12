using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class SideEffect : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Severity { get; set; } // Hafif, Orta, Şiddetli
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<MedicineSideEffect> MedicineSideEffects { get; set; } = new List<MedicineSideEffect>();
    }
}

