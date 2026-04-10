using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace busline_project.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime BookingTime { get; set; } = DateTime.UtcNow;

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalAmount { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    public enum BookingStatus
    {
        Pending = 0,  // Chờ xác nhận / chờ thanh toán
        Confirmed = 1,  // Đã xác nhận
        Cancelled = 2,  // Đã hủy
        Refunded = 3   // Có thể thêm nếu hỗ trợ hoàn tiền
    }
}