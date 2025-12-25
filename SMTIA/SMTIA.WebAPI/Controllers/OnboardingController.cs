using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    // One endpoint to handle the step-by-step registration + initial medicines + schedules
    [AllowAnonymous]
    public sealed class OnboardingController : ApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IJwtProvider _jwtProvider;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            MediatR.IMediator mediator,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationDbContext db,
            IJwtProvider jwtProvider,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<OnboardingController> logger) : base(mediator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _jwtProvider = jwtProvider;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] OnboardingRegisterRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Eksik alan var (ad/soyad/email/şifre)" });
            }

            // ensure User role exists
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));

            var email = request.Email.Trim();
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null) return BadRequest(new { message = "Bu e-posta zaten kayıtlı" });

            var userName = string.IsNullOrWhiteSpace(request.UserName)
                ? (email.Contains('@') ? email.Split('@')[0] : email)
                : request.UserName.Trim();

            var user = new AppUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                UserName = userName,
                Weight = request.WeightKg,
                HeightCm = request.HeightCm,
                Gender = request.Gender,
                // DateOfBirth is optional; if age provided, approximate DOB as Jan 1st of (now-age)
                DateOfBirth = request.AgeYears.HasValue ? new DateTime(DateTime.UtcNow.Year - request.AgeYears.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc) : null,
                BloodType = null
            };

            var create = await _userManager.CreateAsync(user, request.Password);
            if (!create.Succeeded)
            {
                var errors = string.Join(", ", create.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Kayıt başarısız: {errors}" });
            }

            await _userManager.AddToRoleAsync(user, "User");

            // Persist initial medicines + schedules
            if (request.Medicines != null && request.Medicines.Count > 0)
            {
                foreach (var m in request.Medicines)
                {
                    if (string.IsNullOrWhiteSpace(m.Name)) continue;

                    var medName = m.Name.Trim();
                    var medicine = await _db.Medicines.FirstOrDefaultAsync(x => !x.IsDeleted && x.Name.ToLower() == medName.ToLower(), cancellationToken);
                    if (medicine == null)
                    {
                        medicine = new Medicine
                        {
                            Name = medName,
                            DosageForm = m.Type,
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.Medicines.Add(medicine);
                    }

                    var prescription = new UserPrescription
                    {
                        UserId = user.Id,
                        PrescriptionDate = DateTime.UtcNow,
                        StartDate = DateTime.UtcNow,
                        Notes = m.Note,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.UserPrescriptions.Add(prescription);

                    var pm = new PrescriptionMedicine
                    {
                        PrescriptionId = prescription.Id,
                        MedicineId = medicine.Id,
                        Dosage = m.DoseAmount ?? 1,
                        DosageUnit = m.DoseUnit ?? "adet",
                        Quantity = m.PackageSize ?? 0,
                        Instructions = m.Note,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.PrescriptionMedicines.Add(pm);

                    var schedule = new MedicationSchedule
                    {
                        PrescriptionId = prescription.Id,
                        PrescriptionMedicineId = pm.Id,
                        ScheduleName = $"{medicine.Name} Takvimi",
                        StartDate = DateTime.UtcNow,
                        EndDate = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.MedicationSchedules.Add(schedule);

                    var days = m.SelectedDays ?? new List<string>();
                    var times = m.Times ?? new List<OnboardingTimeDto>();

                    if (days.Count > 0 && times.Count > 0)
                    {
                        var daySet = new HashSet<string>(days.Select(x => x.Trim().ToLower()));
                        var isDaily = daySet.Count == 7;

                        foreach (var t in times.Where(x => !string.IsNullOrWhiteSpace(x.Time)))
                        {
                            var timeOnly = TimeOnly.ParseExact(t.Time.Trim(), "HH:mm");
                            var (dosage, unit) = ParseDosageText(t.Dosage);

                            if (isDaily)
                            {
                                _db.ScheduleTimings.Add(new ScheduleTiming
                                {
                                    MedicationScheduleId = schedule.Id,
                                    Time = timeOnly,
                                    Dosage = dosage,
                                    DosageUnit = unit,
                                    DayOfWeek = null,
                                    IntervalHours = null,
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                            else
                            {
                                foreach (var day in daySet)
                                {
                                    _db.ScheduleTimings.Add(new ScheduleTiming
                                    {
                                        MedicationScheduleId = schedule.Id,
                                        Time = timeOnly,
                                        Dosage = dosage,
                                        DosageUnit = unit,
                                        DayOfWeek = ToDayOfWeekInt(day),
                                        IntervalHours = null,
                                        IsActive = true,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Send confirmation email (non-blocking) with HTML template
            _ = Task.Run(async () =>
            {
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = $"https://localhost:7054/api/auth/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
                    
                    var emailBody = $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>SMTIA - E-posta Onayı</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            padding: 40px 20px;
            line-height: 1.6;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            overflow: hidden;
        }}
        
        .email-header {{
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }}
        
        .email-header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(255, 255, 255, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .capsule-icon {{
            width: 80px;
            height: 40px;
            margin: 0 auto 20px;
            border: 3px solid rgba(255, 255, 255, 0.8);
            border-radius: 40px;
            position: relative;
            background: rgba(255, 255, 255, 0.2);
        }}
        
        .capsule-icon::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(255, 255, 255, 0.8);
            transform: translateX(-50%);
        }}
        
        .email-header h1 {{
            color: #ffffff;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 8px;
            position: relative;
            z-index: 1;
        }}
        
        .email-header p {{
            color: rgba(255, 255, 255, 0.95);
            font-size: 16px;
            position: relative;
            z-index: 1;
        }}
        
        .email-body {{
            padding: 48px 40px;
        }}
        
        .greeting {{
            color: #4a4a4a;
            font-size: 18px;
            font-weight: 500;
            margin-bottom: 20px;
        }}
        
        .content {{
            color: #5a5a5a;
            font-size: 16px;
            margin-bottom: 32px;
            line-height: 1.8;
        }}
        
        .button-container {{
            text-align: center;
            margin: 40px 0;
        }}
        
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            color: white !important;
            text-decoration: none;
            padding: 16px 40px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            box-shadow: 0 4px 12px rgba(141, 184, 125, 0.3);
            transition: all 0.3s ease;
        }}
        
        .link-text {{
            color: #7a7a7a;
            font-size: 14px;
            margin-top: 24px;
            padding: 16px;
            background: #f8f6f3;
            border-radius: 8px;
            word-break: break-all;
            font-family: monospace;
        }}
        
        .footer {{
            margin-top: 40px;
            padding-top: 32px;
            border-top: 1px solid #e8e3d9;
            text-align: center;
        }}
        
        .footer p {{
            color: #7a7a7a;
            font-size: 14px;
            margin-bottom: 8px;
        }}
        
        .footer .team-name {{
            color: #a8c8a0;
            font-weight: 600;
            font-size: 16px;
        }}
        
        .expiry-note {{
            color: #9a9a9a;
            font-size: 13px;
            font-style: italic;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <div class=""capsule-icon""></div>
            <h1>SMTIA</h1>
            <p>E-posta Onayı</p>
        </div>
        
        <div class=""email-body"">
            <p class=""greeting"">Merhaba {user.FirstName},</p>
            
            <p class=""content"">
                SMTIA hesabınızı oluşturduğunuz için teşekkür ederiz! E-posta adresinizi onaylamak için aşağıdaki butona tıklayın.
            </p>
            
            <div class=""button-container"">
                <a href=""{confirmationLink}"" class=""button"">E-postamı Onayla</a>
            </div>
            
            <p class=""content"">
                Veya bu linki tarayıcınıza yapıştırın:
            </p>
            
            <div class=""link-text"">
                {confirmationLink}
            </div>
            
            <p class=""expiry-note"">
                ⏰ Bu link 24 saat geçerlidir.
            </p>
            
            <div class=""footer"">
                <p>Saygılarımızla,</p>
                <p class=""team-name"">SMTIA Ekibi</p>
            </div>
        </div>
    </div>
</body>
</html>";
                    
                    await _emailService.SendEmailAsync(user.Email!, "SMTIA - E-posta Onayı", emailBody, CancellationToken.None);
                    _logger.LogInformation("Email confirmation sent successfully to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email confirmation to {Email}", user.Email);
                }
            });

            // Return JSON response for frontend to handle email confirmation message
            return Ok(new
            {
                success = true,
                requiresEmailConfirmation = true,
                email = user.Email,
                message = "Kayıt başarılı! E-posta adresinize onay linki gönderildi. Lütfen e-postanızı kontrol edin."
            });
        }

        private static string GetEmailConfirmationRequiredHtml(string email, string frontendUrl)
        {
            return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>E-posta Onayı Gerekli - SMTIA</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #f5f1eb 0%, #e8e3d9 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        
        .container {{
            background: #ffffff;
            border-radius: 24px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            max-width: 500px;
            width: 100%;
            padding: 48px 40px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        
        .container::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -20%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(200, 180, 160, 0.1) 0%, transparent 70%);
            border-radius: 50%;
        }}
        
        .email-icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 24px;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
            animation: scaleIn 0.5s ease-out;
        }}
        
        @keyframes scaleIn {{
            from {{
                transform: scale(0);
                opacity: 0;
            }}
            to {{
                transform: scale(1);
                opacity: 1;
            }}
        }}
        
        .email-icon::after {{
            content: '✉';
            color: white;
            font-size: 48px;
            font-weight: bold;
        }}
        
        h1 {{
            color: #4a4a4a;
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 12px;
            position: relative;
            z-index: 1;
        }}
        
        .subtitle {{
            color: #7a7a7a;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 32px;
            position: relative;
            z-index: 1;
        }}
        
        .email {{
            color: #5a5a5a;
            font-weight: 500;
            word-break: break-all;
        }}
        
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #a8c8a0 0%, #8db87d 100%);
            color: white;
            text-decoration: none;
            padding: 14px 32px;
            border-radius: 12px;
            font-weight: 500;
            font-size: 16px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(141, 184, 125, 0.3);
            position: relative;
            z-index: 1;
            cursor: pointer;
            border: none;
        }}
        
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(141, 184, 125, 0.4);
        }}
        
        .capsule-decoration {{
            position: absolute;
            bottom: -30px;
            left: 50%;
            transform: translateX(-50%) rotate(-15deg);
            width: 120px;
            height: 60px;
            border: 2px solid rgba(200, 180, 160, 0.3);
            border-radius: 60px;
            opacity: 0.5;
        }}
        
        .capsule-decoration::before {{
            content: '';
            position: absolute;
            left: 50%;
            top: 0;
            width: 2px;
            height: 100%;
            background: rgba(200, 180, 160, 0.3);
        }}
        
        .info-box {{
            background: #f8f6f3;
            border-radius: 12px;
            padding: 20px;
            margin: 24px 0;
            text-align: left;
            position: relative;
            z-index: 1;
        }}
        
        .info-box p {{
            color: #5a5a5a;
            font-size: 14px;
            line-height: 1.6;
            margin-bottom: 8px;
        }}
        
        .info-box p:last-child {{
            margin-bottom: 0;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""email-icon""></div>
        <h1>Kayıt Başarılı!</h1>
        <p class=""subtitle"">
            <span class=""email"">{email}</span> adresine onay e-postası gönderildi.
        </p>
        <div class=""info-box"">
            <p><strong>📧 E-postanızı kontrol edin</strong></p>
            <p>Gelen kutunuzda SMTIA'dan bir e-posta bulacaksınız. E-postadaki linke tıklayarak hesabınızı aktifleştirin.</p>
            <p style=""margin-top: 12px;""><strong>💡 İpucu:</strong> E-posta gelmediyse spam klasörünüzü kontrol edin.</p>
        </div>
        <button class=""button"" onclick=""window.location.href='{frontendUrl}'"">Ana Sayfaya Dön</button>
        <div class=""capsule-decoration""></div>
    </div>
</body>
</html>";
        }

        private static int ToDayOfWeekInt(string dayId)
        {
            return dayId.Trim().ToLower() switch
            {
                "sunday" => 0,
                "monday" => 1,
                "tuesday" => 2,
                "wednesday" => 3,
                "thursday" => 4,
                "friday" => 5,
                "saturday" => 6,
                _ => 1
            };
        }

        private static (decimal dosage, string unit) ParseDosageText(string? dosageText)
        {
            if (string.IsNullOrWhiteSpace(dosageText))
                return (1, "adet");

            var text = dosageText.Trim();
            var digits = new string(text.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            if (!string.IsNullOrWhiteSpace(digits) && decimal.TryParse(digits.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                var rest = text.Substring(digits.Length).Trim();
                if (string.IsNullOrWhiteSpace(rest)) rest = "adet";
                return (d, rest);
            }

            return (1, text);
        }

        public sealed record OnboardingRegisterRequest(
            string FirstName,
            string LastName,
            string Email,
            string Password,
            string? UserName,
            int? AgeYears,
            int? HeightCm,
            decimal? WeightKg,
            string? Gender,
            List<OnboardingMedicineDto>? Medicines);

        public sealed record OnboardingMedicineDto(
            string Name,
            string Type,
            decimal? DoseAmount,
            string? DoseUnit,
            List<string>? SelectedDays,
            List<OnboardingTimeDto>? Times,
            string? Note,
            int? PackageSize);

        public sealed record OnboardingTimeDto(string Time, string? Dosage);
    }
}


