using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SMTIA.Domain.Entities;
using SMTIA.Infrastructure.Context;
using SMTIA.WebAPI.Abstractions;

namespace SMTIA.WebAPI.Controllers
{
    [Authorize]
    public sealed class UserMedicinesController : ApiController
    {
        private readonly ApplicationDbContext _db;

        public UserMedicinesController(MediatR.IMediator mediator, ApplicationDbContext db) : base(mediator)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyMedicines(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            // Load schedules with medicine name + timings
            var schedules = await _db.MedicationSchedules
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.IsActive)
                .Join(_db.UserPrescriptions.AsNoTracking().Where(p => !p.IsDeleted),
                    s => s.PrescriptionId,
                    p => p.Id,
                    (s, p) => new { s, p })
                .Where(x => x.p.UserId == userId.Value)
                .Join(_db.PrescriptionMedicines.AsNoTracking().Where(pm => !pm.IsDeleted),
                    x => x.s.PrescriptionMedicineId,
                    pm => pm.Id,
                    (x, pm) => new { x.s, x.p, pm })
                .Join(_db.Medicines.AsNoTracking().Where(m => !m.IsDeleted),
                    x => x.pm.MedicineId,
                    m => m.Id,
                    (x, m) => new { x.s, x.pm, medicine = m })
                .ToListAsync(cancellationToken);

            var scheduleIds = schedules.Select(x => x.s.Id).Distinct().ToList();

            var timings = await _db.ScheduleTimings
                .AsNoTracking()
                .Where(t => !t.IsDeleted && t.IsActive && scheduleIds.Contains(t.MedicationScheduleId))
                .ToListAsync(cancellationToken);

            var result = schedules.Select(x =>
            {
                var myTimings = timings.Where(t => t.MedicationScheduleId == x.s.Id).ToList();

                // If all timings have DayOfWeek null => daily
                var allDaily = myTimings.Count > 0 && myTimings.All(t => t.DayOfWeek == null);
                var selectedDays = allDaily
                    ? new List<string> { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" }
                    : myTimings.Where(t => t.DayOfWeek.HasValue).Select(t => ToDayId(t.DayOfWeek!.Value)).Distinct().ToList();

                var times = myTimings
                    .OrderBy(t => t.Time)
                    .Select(t => new UserMedicineTimeDto(
                        t.Id.ToString(), // reuse timing id as stable timeId for UI
                        t.Time.ToString("HH:mm"),
                        $"{t.Dosage} {t.DosageUnit}".Trim()))
                    .ToList();

                return new UserMedicineDto(
                    x.s.Id,
                    x.medicine.Id,
                    x.medicine.Name,
                    x.medicine.DosageForm ?? "capsule",
                    new DoseDto(x.pm.Dosage, x.pm.DosageUnit),
                    new ScheduleDto(selectedDays, times));
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddMyMedicine([FromBody] AddUserMedicineRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Medicine name is required" });

            // Find or create medicine from local dataset
            var name = request.Name.Trim();
            var medicine = await _db.Medicines.FirstOrDefaultAsync(m =>
                !m.IsDeleted && m.Name.ToLower() == name.ToLower(), cancellationToken);

            if (medicine == null)
            {
                medicine = new Medicine
                {
                    Name = name,
                    DosageForm = request.Type,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Medicines.Add(medicine);
            }
            else
            {
                // Update dosage form/type if provided
                if (!string.IsNullOrWhiteSpace(request.Type))
                    medicine.DosageForm = request.Type;
            }

            // Create prescription (simple, one per add)
            var prescription = new UserPrescription
            {
                UserId = userId.Value,
                PrescriptionDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                Notes = request.Note,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.UserPrescriptions.Add(prescription);

            var pm = new PrescriptionMedicine
            {
                PrescriptionId = prescription.Id,
                MedicineId = medicine.Id,
                Dosage = request.Dose?.Amount ?? 1,
                DosageUnit = request.Dose?.Unit ?? "adet",
                Quantity = request.PackageSize ?? 0,
                Instructions = request.Note,
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

            // Timings
            var selectedDays = request.Schedule?.SelectedDays ?? new List<string>();
            var times = request.Schedule?.Times ?? new List<ScheduleTimeRequest>();

            // weekly/as-needed -> no timings
            if (selectedDays.Count > 0 && times.Count > 0)
            {
                var daySet = new HashSet<string>(selectedDays.Select(x => x.Trim().ToLower()));
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

            await _db.SaveChangesAsync(cancellationToken);

            // Return DTO the client already expects
            var dto = new UserMedicineDto(
                schedule.Id,
                medicine.Id,
                medicine.Name,
                medicine.DosageForm ?? request.Type ?? "capsule",
                new DoseDto(pm.Dosage, pm.DosageUnit),
                new ScheduleDto(
                    selectedDays.Count == 0
                        ? new List<string>()
                        : selectedDays,
                    (request.Schedule?.Times ?? new List<ScheduleTimeRequest>())
                        .Where(x => !string.IsNullOrWhiteSpace(x.Time))
                        .Select(x => new UserMedicineTimeDto(Guid.NewGuid().ToString(), x.Time.Trim(), x.Dosage ?? ""))
                        .ToList()
                )
            );

            return Ok(dto);
        }

        [HttpPut("{scheduleId:guid}")]
        public async Task<IActionResult> UpdateMyMedicine(Guid scheduleId, [FromBody] AddUserMedicineRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            // Verify ownership
            var schedule = await _db.MedicationSchedules
                .Include(s => s.PrescriptionMedicine)
                .FirstOrDefaultAsync(s => s.Id == scheduleId && !s.IsDeleted, cancellationToken);

            if (schedule == null) return NotFound();

            var prescription = await _db.UserPrescriptions.FirstOrDefaultAsync(p => p.Id == schedule.PrescriptionId && !p.IsDeleted, cancellationToken);
            if (prescription == null || prescription.UserId != userId.Value) return Forbid();

            // Update Medicine/Prescription details
            // Note: We don't change the Medicine ID here (name change not supported in edit for simplicity, user should delete and add new if name changes)
            // But we can update dosage form if needed
            
            var pm = schedule.PrescriptionMedicine;
            if (pm != null)
            {
                if (request.Dose != null)
                {
                    pm.Dosage = request.Dose.Amount;
                    pm.DosageUnit = request.Dose.Unit;
                }
                if (request.PackageSize.HasValue) pm.Quantity = request.PackageSize.Value;
                if (!string.IsNullOrWhiteSpace(request.Note)) pm.Instructions = request.Note;
                
                // Also update prescription notes
                prescription.Notes = request.Note;
            }

            // Update Schedule Timings
            // 1. Soft delete existing timings
            var existingTimings = await _db.ScheduleTimings
                .Where(t => t.MedicationScheduleId == scheduleId && !t.IsDeleted)
                .ToListAsync(cancellationToken);
            
            foreach (var t in existingTimings)
            {
                t.IsDeleted = true;
                t.DeletedAt = DateTime.UtcNow;
                t.IsActive = false;
            }

            // 2. Add new timings
            var selectedDays = request.Schedule?.SelectedDays ?? new List<string>();
            var times = request.Schedule?.Times ?? new List<ScheduleTimeRequest>();

            if (selectedDays.Count > 0 && times.Count > 0)
            {
                var daySet = new HashSet<string>(selectedDays.Select(x => x.Trim().ToLower()));
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

            await _db.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{scheduleId:guid}")]
        public async Task<IActionResult> DeleteMyMedicine(Guid scheduleId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            // Verify ownership by checking prescription->user
            var schedule = await _db.MedicationSchedules.FirstOrDefaultAsync(s => s.Id == scheduleId && !s.IsDeleted, cancellationToken);
            if (schedule == null) return NotFound();

            var prescription = await _db.UserPrescriptions.FirstOrDefaultAsync(p => p.Id == schedule.PrescriptionId && !p.IsDeleted, cancellationToken);
            if (prescription == null || prescription.UserId != userId.Value) return Forbid();

            schedule.IsDeleted = true;
            schedule.DeletedAt = DateTime.UtcNow;
            schedule.IsActive = false;

            // Soft delete timings
            var timings = await _db.ScheduleTimings.Where(t => t.MedicationScheduleId == scheduleId && !t.IsDeleted).ToListAsync(cancellationToken);
            foreach (var t in timings)
            {
                t.IsDeleted = true;
                t.DeletedAt = DateTime.UtcNow;
                t.IsActive = false;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "Deleted" });
        }

        private Guid? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("Id") ?? User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            return Guid.TryParse(userIdClaim.Value, out var id) ? id : null;
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

        private static string ToDayId(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "sunday",
                1 => "monday",
                2 => "tuesday",
                3 => "wednesday",
                4 => "thursday",
                5 => "friday",
                6 => "saturday",
                _ => "monday"
            };
        }

        private static (decimal dosage, string unit) ParseDosageText(string? dosageText)
        {
            if (string.IsNullOrWhiteSpace(dosageText))
                return (1, "adet");

            var text = dosageText.Trim();
            // Common forms: "1 tablet", "2 kapsül", "5ml"
            var digits = new string(text.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            if (!string.IsNullOrWhiteSpace(digits) && decimal.TryParse(digits.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                var rest = text.Substring(digits.Length).Trim();
                if (string.IsNullOrWhiteSpace(rest)) rest = "adet";
                return (d, rest);
            }

            return (1, text);
        }

        public sealed record AddUserMedicineRequest(
            string Name,
            string Type,
            DoseDto? Dose,
            ScheduleRequest? Schedule,
            int? PackageSize,
            string? Note);

        public sealed record DoseDto(decimal Amount, string Unit);

        public sealed record ScheduleRequest(
            List<string> SelectedDays,
            List<ScheduleTimeRequest> Times);

        public sealed record ScheduleTimeRequest(
            string Time,
            string? Dosage);

        public sealed record UserMedicineDto(
            Guid Id,
            Guid MedicineId,
            string Name,
            string Type,
            DoseDto Dose,
            ScheduleDto Schedule);

        public sealed record ScheduleDto(
            List<string> SelectedDays,
            List<UserMedicineTimeDto> Times);

        public sealed record UserMedicineTimeDto(
            string Id,
            string Time,
            string Dosage);
    }
}


