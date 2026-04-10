namespace busline_project.Dtos
{
    public class VehicleSeatDto
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public int? ManufactureYear { get; set; }
        public string Status { get; set; }

        public int VehicleTypeId { get; set; }
        public string TypeName { get; set; }
        public int TotalSeats { get; set; }

        public int SeatTemplateId { get; set; }
        public string SeatCode { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string Deck { get; set; }
        public string SeatType { get; set; }
    }
}
