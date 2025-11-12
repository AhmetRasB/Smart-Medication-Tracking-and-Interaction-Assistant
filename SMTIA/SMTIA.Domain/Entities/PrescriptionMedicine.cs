using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class PrescriptionMedicine : Entity
    {
        public Guid PrescriptionId { get; set; }
        public Guid MedicineId { get; set; }
        public decimal Dosage { get; set; } // Doz miktarı
        public string DosageUnit { get; set; } = string.Empty; // mg, ml, vb.
        public int Quantity { get; set; } // Toplam miktar
        public string? Instructions { get; set; } // Kullanım talimatları
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public UserPrescription Prescription { get; set; } = null!;
        public Medicine Medicine { get; set; } = null!;
    }
}

