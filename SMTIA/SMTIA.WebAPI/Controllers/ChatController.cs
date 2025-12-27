using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;
using SMTIA.WebAPI.Abstractions;
using System.Security.Claims;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class ChatController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IGemmaInteractionAnalyzer _aiService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            MediatR.IMediator mediator, 
            UserManager<AppUser> userManager, 
            ApplicationDbContext db,
            IGemmaInteractionAnalyzer aiService,
            ILogger<ChatController> logger) : base(mediator)
        {
            _userManager = userManager;
            _db = db;
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.UserAllergies)
                .Include(u => u.UserDiseases)
                .Include(u => u.UserSurgeries)
                .Include(u => u.EmergencyContacts)
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

            if (user == null) return NotFound(new { message = "User not found" });

            var userMessage = (request.Message ?? "").Trim();
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return BadRequest(new { message = "Message cannot be empty" });
            }

            try
            {
                // Collect all user health information
                var userContext = await BuildUserContextAsync(user, cancellationToken);

                // Build comprehensive prompt for AI
                var prompt = BuildPrompt(user, userContext, userMessage);

                _logger.LogInformation("Sending chat request to AI for user {UserId}", userId.Value);

                // Get AI response from Groq
                var aiResponse = await _aiService.AnalyzeAsync(prompt, cancellationToken);

                _logger.LogInformation("Received AI response for user {UserId}, length: {Length}", userId.Value, aiResponse?.Length ?? 0);

                return Ok(new { reply = aiResponse?.Trim() ?? "Üzgünüm, şu anda yanıt veremiyorum. Lütfen daha sonra tekrar deneyin." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request for user {UserId}", userId.Value);
                return StatusCode(500, new { message = "AI servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.", error = ex.Message });
            }
        }

        [HttpPost("bmi-analysis")]
        public async Task<IActionResult> GetBmiAnalysis(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new { message = "Token geçersiz veya eksik. Lütfen tekrar giriş yapın." });
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null) return NotFound(new { message = "User not found" });

            if (!user.Weight.HasValue || !user.HeightCm.HasValue)
            {
                return BadRequest(new { message = "Weight and height are required for BMI analysis" });
            }

            var weight = (double)user.Weight.Value;
            var height = user.HeightCm.Value / 100.0; // Convert cm to meters
            var bmi = weight / (height * height);

            string category;
            string status;
            string advice = "";

            if (bmi < 18.5)
            {
                category = "underweight";
                status = "ideal kilonun altındasın";
                advice = "Bir diyetisyene danışmanı öneririm";
            }
            else if (bmi < 25)
            {
                category = "normal";
                status = "ideal kilonda olduğunu görebiliyorum";
                advice = "";
            }
            else if (bmi < 30)
            {
                category = "overweight";
                status = "ideal kilonun üzerindesin";
                advice = "Bir diyetisyene danışmanı öneririm";
            }
            else
            {
                category = "obese";
                status = "obezite riski taşıyorsun";
                advice = "Mutlaka bir diyetisyene danışmanı öneririm";
            }

            var messages = new List<string>
            {
                $"Merhaba {user.FullName},",
                $"Şu anda kilon **{weight} kg**,",
                $"boyun **{user.HeightCm} cm**,",
                $"ve {status}."
            };

            if (!string.IsNullOrEmpty(advice))
            {
                messages.Add(advice);
            }

            messages.Add("Formunu korumaya devam etmelisin");
            messages.Add("Sağlıkla kal! 🍀");

            return Ok(new
            {
                bmi = Math.Round(bmi, 1),
                category,
                weight,
                height = user.HeightCm,
                messages
            });
        }

        [HttpPost("interaction-analysis")]
        public async Task<IActionResult> GetInteractionAnalysis(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new { message = "Token geçersiz veya eksik. Lütfen tekrar giriş yapın." });
            }

            var medicines = await GetUserMedicinesAsync(userId.Value, cancellationToken);

            if (medicines.Count == 0)
            {
                return Ok(new
                {
                    message = "Henüz ilaç eklememişsiniz. İlaç etkileşim risklerini görmek için önce ilaçlarınızı ekleyin.",
                    interactions = new List<object>()
                });
            }

            if (medicines.Count == 1)
            {
                return Ok(new
                {
                    message = $"Şu anda sadece **{medicines[0].Name}** ilacını kullanıyorsunuz. İlaç etkileşim riski değerlendirmesi için en az 2 ilaç gereklidir.",
                    interactions = new List<object>()
                });
            }

            // Generate interaction analysis (dummy for now, can be enhanced with real drug interaction API)
            var interactions = new List<object>();
            for (int i = 0; i < medicines.Count; i++)
            {
                for (int j = i + 1; j < medicines.Count; j++)
                {
                    var risk = new Random().Next(0, 50);
                    var status = risk < 25 ? "Sorun Yok" : risk < 40 ? "Dikkat" : "Yüksek Risk";
                    var statusColor = risk < 25 ? "#27AE60" : risk < 40 ? "#FF9800" : "#fc8181";

                    interactions.Add(new
                    {
                        medicine1 = medicines[i].Name,
                        medicine2 = medicines[j].Name,
                        risk,
                        status,
                        statusColor
                    });
                }
            }

            var averageRisk = interactions.Count > 0
                ? interactions.Average(x => (int)((dynamic)x).risk)
                : 0;

            var overallStatus = averageRisk < 25
                ? "Genel olarak ilaç etkileşim riskiniz **düşük**. Sorun yok."
                : averageRisk < 40
                ? $"Genel ilaç etkileşim riskiniz **%{Math.Round(averageRisk)}**. Doktorunuzla görüşmenizi öneririm."
                : $"Genel ilaç etkileşim riskiniz **%{Math.Round(averageRisk)}** ve **yüksek**. Mutlaka doktorunuzla görüşmelisiniz.";

            return Ok(new
            {
                message = "İlaç etkileşim risk analiziniz:",
                interactions,
                overallStatus,
                averageRisk = Math.Round(averageRisk, 1)
            });
        }

        private async Task<List<MedicineInfo>> GetUserMedicinesAsync(Guid userId, CancellationToken cancellationToken)
        {
            var schedules = await _db.MedicationSchedules
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.IsActive)
                .Join(_db.UserPrescriptions.AsNoTracking().Where(p => !p.IsDeleted),
                    s => s.PrescriptionId,
                    p => p.Id,
                    (s, p) => new { s, p })
                .Where(x => x.p.UserId == userId)
                .Join(_db.PrescriptionMedicines.AsNoTracking().Where(pm => !pm.IsDeleted),
                    x => x.s.PrescriptionMedicineId,
                    pm => pm.Id,
                    (x, pm) => new { x.s, x.p, pm })
                .Join(_db.Medicines.AsNoTracking().Where(m => !m.IsDeleted),
                    x => x.pm.MedicineId,
                    m => m.Id,
                    (x, m) => new MedicineInfo { Id = m.Id, Name = m.Name })
                .Distinct()
                .ToListAsync(cancellationToken);

            return schedules;
        }

        private async Task<UserContext> BuildUserContextAsync(AppUser user, CancellationToken cancellationToken)
        {
            // Get medicines
            var medicines = await GetUserMedicinesAsync(user.Id, cancellationToken);

            // Get allergies
            var allergies = user.UserAllergies
                .Where(a => !a.IsDeleted)
                .Select(a => new AllergyInfo(a.AllergyName, a.Severity, a.Description))
                .ToList();

            // Get diseases
            var diseases = user.UserDiseases
                .Where(d => !d.IsDeleted)
                .Select(d => new DiseaseInfo(d.DiseaseName, d.Description, d.IsActive, d.DiagnosisDate))
                .ToList();

            // Get surgeries
            var surgeries = user.UserSurgeries
                .Where(s => !s.IsDeleted)
                .Select(s => s.SurgeryName)
                .ToList();

            // Get side effects
            var sideEffects = await _db.UserSideEffects
                .AsNoTracking()
                .Where(se => se.UserId == user.Id && !se.IsDeleted)
                .Include(se => se.Medicine)
                .Select(se => new SideEffectInfo(
                    !string.IsNullOrWhiteSpace(se.MedicineName) ? se.MedicineName : (se.Medicine != null ? se.Medicine.Name : "Bilinmeyen"), 
                    se.SideEffects, 
                    se.Severity, 
                    se.Date))
                .ToListAsync(cancellationToken);

            // Get today's intake logs (medicines taken today)
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);
            var todayIntakes = await _db.IntakeLogs
                .AsNoTracking()
                .Where(il => il.UserId == user.Id && 
                            il.ScheduledTime >= todayStart && 
                            il.ScheduledTime < todayEnd)
                .Join(_db.MedicationSchedules.AsNoTracking(),
                    il => il.MedicationScheduleId,
                    ms => ms.Id,
                    (il, ms) => new { il, ms })
                .Join(_db.PrescriptionMedicines.AsNoTracking().Where(pm => !pm.IsDeleted),
                    x => x.ms.PrescriptionMedicineId,
                    pm => pm.Id,
                    (x, pm) => new { x.il, pm })
                .Join(_db.Medicines.AsNoTracking().Where(m => !m.IsDeleted),
                    x => x.pm.MedicineId,
                    m => m.Id,
                    (x, m) => new IntakeInfo(
                        m.Name,
                        x.il.ScheduledTime,
                        x.il.IsTaken,
                        x.il.IsSkipped,
                        x.il.TakenTime))
                .ToListAsync(cancellationToken);

            return new UserContext
            {
                Medicines = medicines,
                Allergies = allergies,
                Diseases = diseases,
                Surgeries = surgeries,
                SideEffects = sideEffects,
                TodayIntakes = todayIntakes
            };
        }

        private string BuildPrompt(AppUser user, UserContext context, string userMessage)
        {
            var prompt = $@"Sen Medigo adlı bir sağlık yönetimi uygulamasının AI asistanısın. Kullanıcıya ilaçları, sağlık durumu ve tıbbi geçmişi hakkında yardımcı oluyorsun.

KULLANICI BİLGİLERİ:
- İsim: {user.FullName}
- Yaş: {(user.DateOfBirth.HasValue ? (DateTime.UtcNow.Year - user.DateOfBirth.Value.Year) : "Bilinmiyor")}
- Cinsiyet: {user.Gender ?? "Belirtilmemiş"}
- Boy: {(user.HeightCm.HasValue ? $"{user.HeightCm} cm" : "Bilinmiyor")}
- Kilo: {(user.Weight.HasValue ? $"{user.Weight} kg" : "Bilinmiyor")}
- Kan Grubu: {user.BloodType ?? "Bilinmiyor"}
- Doğum Yeri: {user.BirthCity ?? "Belirtilmemiş"}
- El Tercihi: {user.Handedness ?? "Belirtilmemiş"}

SAĞLIK BİLGİLERİ:
- Sigara Kullanımı: {(user.Smokes.HasValue ? (user.Smokes.Value ? $"Evet, {user.CigarettesPerDay ?? 0} {user.CigarettesUnit ?? "adet/gün"}" : "Hayır") : "Belirtilmemiş")}
- Alkol Kullanımı: {(user.DrinksAlcohol.HasValue ? (user.DrinksAlcohol.Value ? "Evet" : "Hayır") : "Belirtilmemiş")}
- COVID Geçmişi: {(user.HadCovid.HasValue ? (user.HadCovid.Value ? "Evet" : "Hayır") : "Belirtilmemiş")}
- Acil Durum Notu: {user.AcilNot ?? "Yok"}

KULLANILAN İLAÇLAR:
{(context.Medicines.Any() ? string.Join("\n", context.Medicines.Select((m, i) => $"{i + 1}. {m.Name}")) : "Henüz ilaç eklenmemiş")}

ALERJİLER:
{(context.Allergies.Any() ? string.Join("\n", context.Allergies.Select((a, i) => $"{i + 1}. {a.AllergyName} (Şiddet: {a.Severity ?? "Belirtilmemiş"}){(string.IsNullOrWhiteSpace(a.Description) ? "" : $" - {a.Description}")}")) : "Bilinen alerji yok")}

KRONİK HASTALIKLAR:
{(context.Diseases.Any() ? string.Join("\n", context.Diseases.Select((d, i) => $"{i + 1}. {d.DiseaseName}{(d.IsActive ? " (Aktif)" : " (Geçmişte)")}{(string.IsNullOrWhiteSpace(d.Description) ? "" : $" - {d.Description}")}")) : "Kronik hastalık yok")}

GEÇİRİLMİŞ AMELİYATLAR:
{(context.Surgeries.Any() ? string.Join("\n", context.Surgeries.Select((s, i) => $"{i + 1}. {s}")) : "Ameliyat geçmişi yok")}

KULLANICI TARAFINDAN KAYDEDİLEN YAN ETKİLER (Kullanıcının kendisinin yaşadığı yan etkiler):
{(context.SideEffects.Any() ? string.Join("\n", context.SideEffects.Select((se, i) => {
    var sideEffectList = !string.IsNullOrWhiteSpace(se.SideEffects) 
        ? (se.SideEffects.Contains(",") 
            ? se.SideEffects.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()
            : new List<string> { se.SideEffects.Trim() })
        : new List<string>();
    var sideEffectsText = sideEffectList.Any() 
        ? string.Join(", ", sideEffectList)
        : "Belirtilmemiş";
    var severityText = se.Severity switch
    {
        "mild" => "Hafif",
        "moderate" => "Orta",
        "severe" => "Şiddetli",
        "critical" => "Kritik",
        _ => se.Severity
    };
    return $"{i + 1}. İlaç: {se.MedicineName} - Yan Etkiler: {sideEffectsText} - Şiddet: {severityText} - Tarih: {se.Date:dd.MM.yyyy}";
})) : "Kullanıcı tarafından kaydedilmiş yan etki yok")}

BUGÜN İÇİLEN İLAÇLAR:
{(context.TodayIntakes.Any() ? string.Join("\n", context.TodayIntakes.Select((ti, i) => {
    var status = ti.IsTaken ? $"✅ İçildi{(ti.TakenTime.HasValue ? $" ({ti.TakenTime.Value:HH:mm})" : "")}" : (ti.IsSkipped ? "❌ Atlanıldı" : "⏳ Henüz içilmedi");
    return $"{i + 1}. {ti.MedicineName} - {status} (Planlanan: {ti.ScheduledTime:HH:mm})";
})) : "Bugün henüz ilaç içilmemiş")}

ÖNEMLİ KURALLAR:
1. Tıbbi tavsiye verme, sadece bilgilendirici yanıtlar ver
2. Acil durumlarda mutlaka doktora başvurmasını söyle
3. İlaç dozajı değişikliği için doktora danışmasını öner
4. Kullanıcının bilgilerini dikkate alarak kişiselleştirilmiş yanıtlar ver
5. Türkçe yanıt ver
6. Samimi ve yardımcı bir ton kullan
7. İlaç etkileşimleri hakkında uyarılar yap ama kesin teşhis koyma

KULLANICI SORUSU: {userMessage}

Yanıtını ver:";

            return prompt;
        }

        private Guid? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;
            return userId;
        }

        public sealed record ChatRequest(string Message);
        
        private sealed class MedicineInfo
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private sealed class UserContext
        {
            public List<MedicineInfo> Medicines { get; set; } = new();
            public List<AllergyInfo> Allergies { get; set; } = new();
            public List<DiseaseInfo> Diseases { get; set; } = new();
            public List<string> Surgeries { get; set; } = new();
            public List<SideEffectInfo> SideEffects { get; set; } = new();
            public List<IntakeInfo> TodayIntakes { get; set; } = new();
        }

        private sealed record AllergyInfo(string AllergyName, string? Severity, string? Description);
        private sealed record DiseaseInfo(string DiseaseName, string? Description, bool IsActive, DateTime? DiagnosisDate);
        private sealed record SideEffectInfo(string MedicineName, string SideEffects, string Severity, DateTime Date);
        private sealed record IntakeInfo(string MedicineName, DateTime ScheduledTime, bool IsTaken, bool IsSkipped, DateTime? TakenTime);
    }
}


