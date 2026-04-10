namespace busline_project.Dtos
{
    public class VehicleDetailResponse
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int? ManufactureYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public VehicleTypeInfo VehicleType { get; set; } = new();
        public List<SeatInfo> Seats { get; set; } = new();
    }

    public class VehicleTypeInfo
    {
        public int VehicleTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
    }

    public class SeatInfo
    {
        public int SeatTemplateId { get; set; }
        public string SeatCode { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string Deck { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
    }
}
