using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    /// <summary>
    /// TR marka/ad terimi -> etken madde (TR/EN) eşlemesi.
    /// Ücretsiz mod: Gemma önerir, kullanıcı onayladıkça Confirmed olur ve dataset büyür.
    /// </summary>
    public sealed class MedicineMapping : Entity
    {
        public string QueryTerm { get; set; } = string.Empty; // Kullanıcının yazdığı: "Parol"
        public string? BrandNameTr { get; set; } // "Parol"
        public string? ActiveIngredientTr { get; set; } // "Parasetamol"
        public string? ActiveIngredientEn { get; set; } // "acetaminophen" (openFDA için)

        public MappingStatus Status { get; set; } = MappingStatus.Pending;
        public MappingSource Source { get; set; } = MappingSource.GemmaSuggested;
        public decimal Confidence { get; set; } = 0.0m; // 0-1 arası öneri güveni

        public Guid? ConfirmedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation (opsiyonel)
        public AppUser? ConfirmedByUser { get; set; }
    }

    public enum MappingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Rejected = 2
    }

    public enum MappingSource
    {
        GemmaSuggested = 0,
        UserConfirmed = 1,
        Imported = 2
    }
}


