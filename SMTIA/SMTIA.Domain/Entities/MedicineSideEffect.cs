using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class MedicineSideEffect : Entity
    {
        public Guid MedicineId { get; set; }
        public Guid SideEffectId { get; set; }
        public string? Frequency { get; set; } // Sık, Nadir, Çok Nadir

        // Navigation properties
        public Medicine Medicine { get; set; } = null!;
        public SideEffect SideEffect { get; set; } = null!;
    }
}

