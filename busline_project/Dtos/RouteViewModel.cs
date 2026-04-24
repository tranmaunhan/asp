namespace busline_project.Dtos
{
    public class RouteViewModel
    {
        public int RouteId { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int? DistanceKm { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string RoutePath { get; set; }
    }
}
