using SMTIA.Domain.Abstractions;

namespace SMTIA.Domain.Entities
{
    public sealed class InteractionAnalysis : Entity
    {
        public Guid UserId { get; set; }
        public Guid? NewMedicineId { get; set; } // Analiz edilen yeni ilaç (opsiyonel)
        public string? NewMedicineName { get; set; } // Eğer ilaç veritabanında yoksa isim
        public string ExistingMedicinesJson { get; set; } = string.Empty; // JSON formatında mevcut ilaç listesi
        public string? AllergiesJson { get; set; } // JSON formatında alerji listesi
        public RiskLevel RiskLevel { get; set; }
        public string Summary { get; set; } = string.Empty; // AI'dan gelen özet analiz
        public string? DetailedAnalysis { get; set; } // Detaylı analiz metni
        public string? Recommendations { get; set; } // Öneriler
        public string? RawAiResponse { get; set; } // Ham AI yanıtı (debug için)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AppUser User { get; set; } = null!;
        public Medicine? NewMedicine { get; set; }
    }

    public enum RiskLevel
    {
        None = 0,        // Risk yok
        Low = 1,        // Düşük risk
        Medium = 2,     // Orta risk
        High = 3,       // Yüksek risk
        Critical = 4    // Kritik risk
    }
}

