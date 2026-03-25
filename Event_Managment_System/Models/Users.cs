using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [DisplayName("Image")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        // Navigation Property
        public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
    }
}
