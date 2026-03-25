using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class PortfolioImages
    {
        [Key]
        public int PortfolioImageId { get; set; }

        [DisplayName("Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        public int PortfolioId { get; set; }

        [ForeignKey("PortfolioId")]
        public Portfolios Portfolio { get; set; }
    }
}
