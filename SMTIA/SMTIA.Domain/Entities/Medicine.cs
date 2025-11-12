using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class Medicine : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DosageForm { get; set; } // Tablet, Kapsül, Şurup, vb.
        public string? ActiveIngredient { get; set; }
        public string? Manufacturer { get; set; }
        public string? Barcode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<MedicineSideEffect> MedicineSideEffects { get; set; } = new List<MedicineSideEffect>();
        public ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
    }
}

