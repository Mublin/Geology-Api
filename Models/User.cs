using System.ComponentModel.DataAnnotations;

namespace Geology_Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    [Required]
    public string RegistrationNumber { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public bool IsAdmin { get; set; } = false;
    public bool IsSuperAdmin { get; set; } = false;
    public bool IsStudent { get; set; } = true;
    public bool IsActivated { get; set; } = false;

    public bool IsLecturer { get; set; } = false;
    public Hash Hash { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
