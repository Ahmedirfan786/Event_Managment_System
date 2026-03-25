using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        [DisplayName("Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        // Navigation property
        public ICollection<Packages> Packages { get; set; } = new List<Packages>();
        public ICollection<Portfolios> Portfolios { get; set; } = new List<Portfolios>();
    }
}
