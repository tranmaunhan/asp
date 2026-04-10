using busline_project.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace busline_project.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string LicensePlate { get; set; } = string.Empty;  // Biển số duy nhất

        [MaxLength(50)]
        public string? Brand { get; set; }  // Hyundai, Thaco, Samco...

        [Range(1900, 2100)]
        public int? ManufactureYear { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }

        [Required]
        public VehicleStatus Status { get; set; } = VehicleStatus.Active;

        // Navigation
        public VehicleType VehicleType { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }

    public enum VehicleStatus
    {
        Active = 0,       // Sẵn sàng chạy
        Maintenance = 1   // Đang bảo dưỡng
    }
}
