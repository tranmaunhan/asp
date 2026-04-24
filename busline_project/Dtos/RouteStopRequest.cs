namespace busline_project.Dtos
{
    public class RouteStopDto
    {
        public int LocationId { get; set; }
        public int StopOrder { get; set; }

        public int? DistanceFromStartKm { get; set; }
        public int? EstimatedTimeFromStartMinutes { get; set; }
    }
}
