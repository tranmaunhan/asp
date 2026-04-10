using System.Text.Json.Serialization;
namespace busline_project.Models
{
    public class UserRole
    {
        // Khóa chính composite
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation properties (required cho EF Core tracking)
        [JsonIgnore]
        public User User { get; set; } = null!;
        [JsonIgnore]
        public Role Role { get; set; } = null!;
    }
}