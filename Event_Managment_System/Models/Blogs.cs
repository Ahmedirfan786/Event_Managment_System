using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class Blogs
    {
        [Key]
        public int BlogId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [DisplayName("Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }
    }
}
