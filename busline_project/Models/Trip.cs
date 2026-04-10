using System.ComponentModel.DataAnnotations;

namespace busline_project.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RouteId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public TripStatus Status { get; set; } = TripStatus.Scheduled;

        // Navigation properties
        public Route Route { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public ICollection<TripSeat> TripSeats { get; set; } = new List<TripSeat>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    public enum TripStatus
    {
        Scheduled = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}
