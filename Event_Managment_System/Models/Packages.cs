using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class Packages
    {
        [Key]
        public int PackageId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Categories Category { get; set; }

        // Navigation property
        public ICollection<PackageFeatures> PackageFeatures { get; set; } = new List<PackageFeatures>();
    }
}
