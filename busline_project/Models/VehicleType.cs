using System.ComponentModel.DataAnnotations;

namespace busline_project.Models
{
    public class VehicleType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;  

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalSeats { get; set; }  

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<SeatTemplate> SeatTemplates { get; set; } = new List<SeatTemplate>();
    }
}