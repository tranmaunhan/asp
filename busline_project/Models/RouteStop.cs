using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace busline_project.Models
{
    public class RouteStop
    {
        [Key]
        public int Id { get; set; }

        public int RouteId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int StopOrder { get; set; }

        [Range(0, 10000)]
        public int? DistanceFromStartKm { get; set; }

        [Range(0, 1440)]
        public int? EstimatedTimeFromStartMinutes { get; set; }

        [JsonIgnore]
        public Route? Route { get; set; }

        [JsonIgnore]
        public Location? Location { get; set; }
    }
}