using Microsoft.AspNetCore.Identity;

namespace SMTIA.Domain.Entities
{
    public sealed class AppUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => string.Join(" ", FirstName, LastName);
        public DateTime? DateOfBirth { get; set; }
        public decimal? Weight { get; set; }
        public string? BloodType { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }

        // Navigation properties
        public ICollection<UserAllergy> UserAllergies { get; set; } = new List<UserAllergy>();
        public ICollection<UserDisease> UserDiseases { get; set; } = new List<UserDisease>();
    }
}
