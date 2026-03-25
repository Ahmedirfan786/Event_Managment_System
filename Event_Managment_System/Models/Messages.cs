using System.ComponentModel.DataAnnotations;

namespace Event_Managment_System.Models
{
    public class Messages
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string Name { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        [MaxLength(1500)]
        public string Message { get; set; }
    }
}
