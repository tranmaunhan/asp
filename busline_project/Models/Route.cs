using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [Range(0, 1440)]
        public int? EstimatedDurationMinutes { get; set; }

        [JsonIgnore]
        public Location? Origin { get; set; }

        [JsonIgnore]
        public Location? Destination { get; set; }

        public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();

        [JsonIgnore]
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}