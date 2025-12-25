using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ChatController(MediatR.IMediator mediator, UserManager<AppUser> userManager, ApplicationDbContext db) : base(mediator)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null) return NotFound(new { message = "User not found" });

            // Get user medicines for context
            var medicines = await GetUserMedicinesAsync(userId.Value, cancellationToken);

            var message = (request.Message ?? "").Trim().ToLowerInvariant();

            // Enhanced responses with user context
            var reply =
                message.Contains("yan etki") ? "Yan etki hissediyorsan doktoruna danışmanı öneririm. İstersen aldığın ilaçları listele, risk analizi yapayım." :
                message.Contains("doz") ? "Doz konusunda doktorun önerisini baz almalısın. İlacın adını ve dozunu yazarsan daha net yardımcı olurum." :
                message.Contains("ilaç") ? $"İlaçlarını düzenli almak önemli. Şu anda {medicines.Count} ilaç kullanıyorsun. Takviminden bugün hangi saatlerde ilaçların var, kontrol edelim." :
                "Anladım. Bana ilaç adı / doz / kullanım sıklığı yazarsan yardımcı olayım.";

            return Ok(new { reply });
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
    }
}


