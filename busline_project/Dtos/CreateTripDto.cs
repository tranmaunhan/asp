using busline_project.Models;

namespace busline_project.Dtos
{
    public class CreateTripDto
    {
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public DateTime DepartureTime { get; set; }
        public TripStatus Status { get; set; }
    }
}
