using System.ComponentModel.DataAnnotations;

namespace busline_project.Models
{
    public class RouteStop
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RouteId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int StopOrder { get; set; }  // 1 = xuất phát, 2, 3..., n = đích

        [Range(0, 10000)]
        public int? DistanceFromStartKm { get; set; }

        [Range(0, 1440)]
        public int? EstimatedTimeFromStartMinutes { get; set; }

        // Navigation
        public Route Route { get; set; } = null!;
        public Location Location { get; set; } = null!;
    }
}