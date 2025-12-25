using Microsoft.AspNetCore.Identity;

namespace SMTIA.Domain.Entities
{
    public sealed class AppUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => string.Join(" ", FirstName, LastName);
        public DateTime? DateOfBirth { get; set; }
        public int? HeightCm { get; set; }
        public decimal? Weight { get; set; }
        public string? Gender { get; set; } // male/female/other (or UI values)
        public string? BloodType { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }

        public string? TcIdentityNo { get; set; }
        public bool? Smokes { get; set; }
        public int? CigarettesPerDay { get; set; }
        public string? CigarettesUnit { get; set; }
        public bool? DrinksAlcohol { get; set; }
        public bool? HadCovid { get; set; }
        public string? BirthCity { get; set; }
        public string? AcilNot { get; set; }
        public string? Handedness { get; set; } // Right/Left

        // Navigation properties
        public ICollection<UserAllergy> UserAllergies { get; set; } = new List<UserAllergy>();
        public ICollection<UserDisease> UserDiseases { get; set; } = new List<UserDisease>();
        public ICollection<UserEmergencyContact> EmergencyContacts { get; set; } = new List<UserEmergencyContact>();
        public ICollection<UserSideEffect> UserSideEffects { get; set; } = new List<UserSideEffect>();
        
        // Surgeries can be stored as UserDisease with a flag or separate entity, 
        // but for simplicity and matching frontend "surgeries" list, we can use a simple list of strings stored as JSON or similar.
        // However, EF Core doesn't support primitive collections easily without value converters.
        // We will use a separate entity for Surgeries to be clean.
        public ICollection<UserSurgery> UserSurgeries { get; set; } = new List<UserSurgery>();
    }
}
