using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace busline_project.Models
{
    public class TripSeat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public int SeatTemplateId { get; set; }

        [Required]
        public TripSeatStatus Status { get; set; } = TripSeatStatus.Available;

        // Navigation properties
        public Trip Trip { get; set; } = null!;
        public SeatTemplate SeatTemplate { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    public enum TripSeatStatus
    {
        Available = 0,   // Còn trống
        Reserved = 1,   // Đang giữ chỗ tạm thời (ví dụ: đang thanh toán)
        Booked = 2,   // Đã bán / đã đặt
        Blocked = 3    // Khóa ghế (hỏng, dành riêng, không bán)
    }
}