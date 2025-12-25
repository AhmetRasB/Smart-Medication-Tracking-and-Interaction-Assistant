# Changelog - 20 Aralık 2024

## 📋 Özet

Bu gün yapılan değişiklikler, AI entegrasyonu, veritabanı migration'ları ve ilaç arama sisteminin iyileştirilmesi üzerine odaklanmıştır.

---

## 🔄 Yapılan Değişiklikler

### 1. Veritabanı Migration'ı

**Problem:** `MedicineMappings` tablosu veritabanında mevcut değildi ve `relation "MedicineMappings" does not exist` hatası alınıyordu.

**Çözüm:**
- `AddMedicineMappings` migration'ı uygulandı
- `MedicineMappings` tablosu oluşturuldu

**Komut:**
```bash
dotnet ef database update --project SMTIA.Infrastructure --startup-project SMTIA.WebAPI --no-build
```

**Sonuç:** ✅ Migration başarıyla uygulandı, tablo oluşturuldu.

---

### 2. AI Entegrasyonu: Hugging Face → Groq

**Problem:** 
- Hugging Face eski API endpoint'i (`api-inference.huggingface.co`) artık desteklenmiyor
- 410 Gone hatası alınıyordu
- Router endpoint'i (`router.huggingface.co`) çalışmıyordu

**Çözüm:**
- **Groq API**'ye geçiş yapıldı (ücretsiz, hızlı ve güvenilir)
- OpenAI uyumlu chat completions formatı kullanılıyor
- Model: `llama-3.1-8b-instant` (ücretsiz, hızlı, güçlü)

**Değiştirilen Dosyalar:**
- `SMTIA.Infrastructure/Options/GemmaOptions.cs`
- `SMTIA.Infrastructure/Services/GemmaInteractionAnalyzer.cs`
- `SMTIA.WebAPI/appsettings.json`
- `SMTIA.WebAPI/appsettings.Development.json`

**Yeni Yapılandırma:**
```json
{
  "Gemma": {
    "ApiToken": "",
    "ModelName": "llama-3.1-8b-instant",
    "BaseUrl": "https://api.groq.com/openai/v1/chat/completions",
    "MaxTokens": 1000,
    "Temperature": 0.7
  }
}
```

**API Key Ekleme:**
```powershell
cd C:\Projects\SMTIA\SMTIA\SMTIA.WebAPI
dotnet user-secrets set "Gemma:ApiToken" "GROQ_API_KEY_BURAYA"
```

**Groq API Key Alma:**
1. https://console.groq.com/ adresine git
2. Sign up/Login yap
3. API Keys bölümünden yeni key oluştur
4. Key'i kopyala ve user-secrets'e ekle

**Sonuç:** ✅ Groq API entegrasyonu tamamlandı (API key eklenmesi gerekiyor).

---

### 3. İlaç Arama: AI Kaldırıldı

**Problem:** 
- İlaç arama işlemi AI kullanıyordu
- Kullanıcı normal veritabanı sorgusu istedi
- AI mapping suggestion'ları gereksizdi

**Çözüm:**
- `SmartSearchMedicinesQueryHandler`'dan AI kaldırıldı
- Sadece normal veritabanı sorgusu yapılıyor
- `MedicineMapping` ve AI suggestion mantığı kaldırıldı
- Yan etkiler (SideEffects) eklendi

**Değiştirilen Dosyalar:**
- `SMTIA.Application/Features/Medicines/SmartSearch/SmartSearchMedicinesQueryHandler.cs`
- `SMTIA.Application/Features/Medicines/SmartSearch/SmartSearchMedicinesQueryResponse.cs`

**Yeni Response Yapısı:**
```csharp
public sealed record LocalMedicineDto(
    Guid Id,
    string Name,
    string? ActiveIngredient,
    string? DosageForm,
    string? Manufacturer,
    string? Barcode,
    string? Description,
    List<SideEffectDto> SideEffects);

public sealed record SideEffectDto(
    Guid Id,
    string Name,
    string? Description,
    string? Severity,
    string? Frequency);
```

**Sonuç:** ✅ İlaç arama tamamen veritabanı tabanlı, yan etkiler dahil.

---

### 4. Yan Etkiler Eklendi

**Yapılanlar:**
- `LocalMedicineDto`'ya `SideEffects` listesi eklendi
- `SideEffectDto` oluşturuldu
- `SmartSearchMedicinesQueryHandler` yan etkileri de getiriyor

**Response Örneği:**
```json
{
  "data": {
    "localMedicines": [
      {
        "id": "guid-buraya",
        "name": "Parol",
        "activeIngredient": "Parasetamol",
        "dosageForm": "Tablet",
        "manufacturer": "Sanofi",
        "barcode": "8690123456789",
        "description": "Ağrı kesici ve ateş düşürücü",
        "sideEffects": [
          {
            "id": "guid-buraya",
            "name": "Mide bulantısı",
            "description": "Nadiren görülen yan etki",
            "severity": "Hafif",
            "frequency": "Nadir"
          }
        ]
      }
    ],
    "localTotal": 1,
    "mappingSuggestion": null,
    "openFdaMedicines": null,
    "openFdaTotal": null
  }
}
```

**Sonuç:** ✅ Yan etkiler ilaç arama sonuçlarına dahil edildi.

---

### 5. User-Secrets Yapılandırması

**Problem:** `UserSecretsId` property'si `.csproj` dosyasında eksikti.

**Çözüm:**
- `SMTIA.WebAPI.csproj`'a `UserSecretsId` eklendi
- Groq API key için user-secrets kullanımı hazırlandı

**Değiştirilen Dosya:**
- `SMTIA.WebAPI/SMTIA.WebAPI.csproj`

**Eklenen Property:**
```xml
<UserSecretsId>smtia-webapi-secrets</UserSecretsId>
```

**Sonuç:** ✅ User-secrets yapılandırması tamamlandı.

---

## 📝 API Endpoint Dokümantasyonu

### Tüm POST Endpoint'leri

#### 1. İlaç (Medicine) Kayıtları

**POST** `/api/medicines`
```json
{
  "name": "Parol",
  "description": "Ağrı kesici ve ateş düşürücü",
  "dosageForm": "Tablet",
  "activeIngredient": "Parasetamol",
  "manufacturer": "Sanofi",
  "barcode": "8690123456789"
}
```

**POST** `/api/medicines/add-to-user`
```json
{
  "userId": "019ab58b-ef60-714c-b664-84889f76c683",
  "medicineName": "Parol",
  "dosage": 500,
  "dosageUnit": "mg",
  "packageSize": 20,
  "dailyDoseCount": 3,
  "doctorNote": "Yemeklerden sonra alın"
}
```

#### 2. Reçete (Prescription) Kayıtları

**POST** `/api/prescriptions`
```json
{
  "userId": "019ab58b-ef60-714c-b664-84889f76c683",
  "doctorName": "Dr. Ahmet Yılmaz",
  "doctorSpecialty": "Kardiyoloji",
  "prescriptionNumber": "REC-2024-001",
  "prescriptionDate": "2024-01-15T10:00:00Z",
  "startDate": "2024-01-15T10:00:00Z",
  "endDate": "2024-02-15T10:00:00Z",
  "notes": "Düzenli kullanılmalı",
  "medicines": [
    {
      "medicineId": "ilac-guid-buraya",
      "dosage": 500,
      "dosageUnit": "mg",
      "quantity": 30,
      "instructions": "Günde 3 kez, yemeklerden sonra"
    }
  ]
}
```

#### 3. İlaç Takvimi (Schedule) Kayıtları

**POST** `/api/schedules`
```json
{
  "userId": "019ab58b-ef60-714c-b664-84889f76c683",
  "prescriptionId": "reçete-guid-buraya",
  "prescriptionMedicineId": "reçete-ilaç-guid-buraya",
  "scheduleName": "Sabah-Akşam",
  "startDate": "2024-01-15T08:00:00Z",
  "endDate": "2024-02-15T20:00:00Z",
  "dosage": 500,
  "dosageUnit": "mg",
  "timingRule": {
    "type": "Daily",
    "intervalHours": null,
    "daysOfWeek": null,
    "time": null,
    "dailyTimes": ["08:00", "20:00"]
  }
}
```

**TimingRule Örnekleri:**

- **Daily (Günlük):**
```json
{
  "type": "Daily",
  "dailyTimes": ["08:00", "14:00", "20:00"]
}
```

- **Weekly (Haftalık):**
```json
{
  "type": "Weekly",
  "daysOfWeek": [1, 3, 5],
  "time": "09:00"
}
```

- **Interval (Aralıklı):**
```json
{
  "type": "Interval",
  "intervalHours": 12
}
```

#### 4. İlaç Alım Kaydı (IntakeLog)

**POST** `/api/intakelogs`
```json
{
  "scheduleId": "takvim-guid-buraya",
  "userId": "019ab58b-ef60-714c-b664-84889f76c683",
  "scheduledTime": "2024-01-15T08:00:00Z",
  "takenTime": "2024-01-15T08:05:00Z",
  "isTaken": true,
  "isSkipped": false,
  "notes": "Zamanında alındı"
}
```

**POST** `/api/intakelogs/{logId}/markAsTaken?userId={userId}`
```json
{
  "isTaken": true,
  "isSkipped": false,
  "takenTime": "2024-01-15T08:05:00Z",
  "notes": "Alındı"
}
```

#### 5. Alerji (Allergy) Kayıtları

**POST** `/api/allergies`
```json
{
  "userId": "019ab58b-ef60-714c-b664-84889f76c683",
  "allergyName": "Penisilin",
  "description": "Ciddi alerjik reaksiyon",
  "severity": "Şiddetli"
}
```

#### 6. Hastalık (Disease) Kayıtları

**POST** `/api/diseases`
```json
{
  "userId": "019ab58b-ef60-714c-b664-84889f76c683",
  "diseaseName": "Hipertansiyon",
  "description": "Yüksek tansiyon",
  "diagnosisDate": "2023-06-01T00:00:00Z"
}
```

#### 7. İlaç Eşleştirme Onayı (MedicineMapping)

**POST** `/api/medicinemappings/confirm`
```json
{
  "mappingId": "4c0b8ce2-b95d-41a4-80c0-0857a4b0e6bc",
  "confirmed": true
}
```

#### 8. İlaç Etkileşim Analizi (AI)

**POST** `/api/interactions/analyze`
```json
{
  "newMedicineId": "ilac-guid-buraya",
  "newMedicineName": "Parol"
}
```

**Not:** Bu endpoint AI kullanır (Groq). UserId JWT token'dan alınır.

#### 9. Kullanıcı Kaydı (Auth)

**POST** `/api/auth/register`
```json
{
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "email": "ahmet@example.com",
  "userName": "ahmetyilmaz7",
  "password": "SecurePass123!",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "weight": 75.5,
  "bloodType": "A+",
  "allergies": [
    {
      "allergyName": "Penisilin",
      "description": "Alerji",
      "severity": "Orta"
    }
  ],
  "diseases": [
    {
      "diseaseName": "Hipertansiyon",
      "description": "Yüksek tansiyon",
      "diagnosisDate": "2023-06-01T00:00:00Z"
    }
  ]
}
```

**POST** `/api/auth/login`
```json
{
  "email": "ahmet@example.com",
  "password": "SecurePass123!"
}
```

---

## ⚠️ Bilinen Eksiklikler

### SideEffect ve MedicineSideEffect Endpoint'leri

**Durum:** SideEffect ve MedicineSideEffect için CRUD endpoint'leri henüz oluşturulmadı.

**Geçici Çözüm:**
- Manuel SQL ile veri eklenebilir
- Veya endpoint'ler eklenecek (gelecek güncellemede)

**Önerilen Endpoint'ler:**
- `POST /api/sideeffects` - Yan etki oluştur
- `POST /api/medicines/{medicineId}/sideeffects` - İlaça yan etki ekle

---

## 🚀 Yapılması Gerekenler

### 1. Groq API Key Ekleme

```powershell
cd C:\Projects\SMTIA\SMTIA\SMTIA.WebAPI
dotnet user-secrets set "Gemma:ApiToken" "GROQ_API_KEY_BURAYA"
```

**Groq API Key Alma:**
1. https://console.groq.com/ adresine git
2. Sign up/Login yap
3. API Keys bölümünden yeni key oluştur
4. Key'i kopyala ve user-secrets'e ekle

### 2. Veritabanına Test Verileri Ekleme

**İlaç Kaydı:**
```bash
POST /api/medicines
{
  "name": "Parol",
  "activeIngredient": "Parasetamol",
  "dosageForm": "Tablet",
  "manufacturer": "Sanofi"
}
```

**Yan Etki Kayıtları:**
- Manuel SQL ile veya gelecek endpoint ile eklenecek

### 3. WebAPI Restart

Groq API key'i ekledikten sonra WebAPI'yi restart et.

---

## 📊 Özet Tablosu

| Değişiklik | Durum | Dosya Sayısı |
|------------|-------|--------------|
| Veritabanı Migration | ✅ Tamamlandı | 1 |
| Groq API Entegrasyonu | ✅ Tamamlandı | 4 |
| İlaç Arama AI Kaldırma | ✅ Tamamlandı | 2 |
| Yan Etkiler Ekleme | ✅ Tamamlandı | 2 |
| User-Secrets Yapılandırma | ✅ Tamamlandı | 1 |
| API Dokümantasyonu | ✅ Tamamlandı | 1 |

**Toplam Değiştirilen Dosya:** 11

---

## 🔗 İlgili Linkler

- **Groq Console:** https://console.groq.com/
- **Groq API Docs:** https://console.groq.com/docs
- **Swagger UI:** https://localhost:7054/swagger

---

## 📝 Notlar

- AI artık sadece `/api/interactions/analyze` endpoint'inde kullanılıyor
- İlaç arama tamamen veritabanı tabanlı
- Groq API key eklenene kadar AI özellikleri çalışmayacak
- SideEffect ve MedicineSideEffect endpoint'leri gelecek güncellemede eklenecek

---

**Dokümantasyon Tarihi:** 20 Aralık 2024  
**Versiyon:** 1.0.0

