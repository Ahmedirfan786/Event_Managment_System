using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Managment_System.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [DisplayName("ProfileImage")]
        public string? ProfileImagePath { get; set; }

        [NotMapped]
        public IFormFile? ProfileImage { get; set; }

    }
}
