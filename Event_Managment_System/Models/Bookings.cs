using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Event_Managment_System.Models
{
    public class Bookings
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Booking Date")]
        public DateTime BookDate { get; set; }

        [Required]
        public string PackageName { get; set; }

        [Required]
        public int PackagePrice { get; set; }

        [Required]
        public string PackageFeatures { get; set; }

        [Required]
        public string PackageCategory { get; set; }

        public string? Remarks { get; set; }

        [Required]
        public string Status { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

    }
}
