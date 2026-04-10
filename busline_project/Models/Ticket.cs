using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace busline_project.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public int TripSeatId { get; set; }

        [Required]
        public int PickupStopId { get; set; }

        [Required]
        public int DropoffStopId { get; set; }

        // Có thể thêm trường giá vé cho vé này (nếu giá khác nhau theo đoạn đường)
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;
        public Trip Trip { get; set; } = null!;
        public TripSeat TripSeat { get; set; } = null!;
        public RouteStop PickupStop { get; set; } = null!;
        public RouteStop DropoffStop { get; set; } = null!;
    }
}