using busline_project.Models;

namespace busline_project.Dtos
{
    public class UpdateTripDto
    {
        public DateTime DepartureTime { get; set; }
        public TripStatus Status { get; set; }
    }
}
