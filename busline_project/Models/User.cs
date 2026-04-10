using System.ComponentModel.DataAnnotations;

namespace busline_project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;  // bcrypt / argon2id

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property: Một User có thể có nhiều Role thông qua UserRole
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Navigation property: Một User có thể có nhiều Booking
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
