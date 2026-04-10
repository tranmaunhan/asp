using System.ComponentModel.DataAnnotations;

namespace busline_project.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;  // "Bến xe Miền Đông", "TP. Hồ Chí Minh", "Trạm dừng QL1A - Bình Dương"...

        [MaxLength(255)]
        public string? Address { get; set; }  // Địa chỉ chi tiết (nếu có)

        [Required]
        public LocationType Type { get; set; }  // station, city, stop

        // Navigation properties (dùng cho routes)
        public ICollection<Route> OriginRoutes { get; set; } = new List<Route>();
        public ICollection<Route> DestinationRoutes { get; set; } = new List<Route>();
        public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
    }

    public enum LocationType
    {
        Station = 0,   // Bến xe
        City = 1,  // Thành phố / tỉnh
        Stop = 2   // Điểm dừng dọc đường
    }
}