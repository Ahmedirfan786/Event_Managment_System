using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class Portfolios
    {
        [Key]
        public int PortfolioId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, StringLength(2500)]
        public string Description { get; set; }

        [Required]
        public int Budget { get; set;}

        [Required]
        public string Location { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Event Date")]
        public DateTime EventDate { get; set; }

        [DisplayName("Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Categories Category { get; set; }

        // Navigation property
        public ICollection<PortfolioImages> PortfolioImages { get; set; } = new List<PortfolioImages>();

    }
}
