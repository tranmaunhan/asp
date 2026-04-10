using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace busline_project.Models
{
    public class SeatTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }

        [Required]
        [MaxLength(10)]
        public string SeatCode { get; set; } = string.Empty;
       

        public int RowIndex { get; set; }   
        public int ColIndex { get; set; }  

        [Required]
        [MaxLength(10)]
        public string Deck { get; set; } = "LOWER";
  
        [MaxLength(20)]
        public string SeatType { get; set; } = "BED";

        [ForeignKey("VehicleTypeId")]
        public VehicleType VehicleType { get; set; } = null!;
    }
}