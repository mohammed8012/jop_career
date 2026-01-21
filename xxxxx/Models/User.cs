using System.ComponentModel.DataAnnotations;

namespace xxxxx.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // Hash in production

        public string Bio { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    }

    // DTOs (Data Transfer Objects)
    public record LoginDto(string Email, string Password);
    public record RegisterDto(string Name, string Email, string Password);
    public record UpdateProfileDto(string Name, string Bio);
}
