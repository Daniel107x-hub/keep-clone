using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User{
        [Key]
        public long Id { get; set; }
    
        [MaxLength(50)]
        public required string FirstName { get; set; }
        [MaxLength(100)]
        public required string LastName { get; set; }
        [EmailAddress]
        [MaxLength(100)]
        public required string Email { get; set; }
        [MaxLength(100)]
        public required string Password { get; set; }
        [Phone]
        [MaxLength(20)]
        public required string PhoneNumber { get; set; }
        [MaxLength(50)]
        public required string UserName { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Note> Notes { get; }
    }
}