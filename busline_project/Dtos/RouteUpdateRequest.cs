namespace busline_project.Dtos
{
    public class RouteUpdateDto
    {
        public int OriginId { get; set; }
        public int DestinationId { get; set; }

        public int? DistanceKm { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        public List<RouteStopDto> RouteStops { get; set; }
    }
}
