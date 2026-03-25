using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class PackageFeatures
    {
        [Key]
        public int FeatureId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int PackageId { get; set; }

        [ForeignKey("PackageId")]
        public Packages Package { get; set; }


    }
}
