using System.ComponentModel.DataAnnotations;

namespace busline_project.Models
{
    public class Route
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OriginId { get; set; }

        [Required]
        public int DestinationId { get; set; }

        [Range(0, 10000)]
        public int? DistanceKm { get; set; }

        [Range(0, 1440)]  // tối đa 24h = 1440 phút
        public int? EstimatedDurationMinutes { get; set; }

        // Navigation
        public Location Origin { get; set; } = null!;
        public Location Destination { get; set; } = null!;
        public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}