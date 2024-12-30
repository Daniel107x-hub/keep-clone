using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("Notes")]
// [PrimaryKey(nameof(Id), nameof(Title))] In case of composite primary key
    public class Note {
        [Key]
        public long Id { get; set; } // For numeric keys, by default, EF Core interprets the key as an identity column and generates a value for it

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public required string Content { get; set; }

        public long UserId { get; set; } // Foreign key
        public User User { get; set; }
    }   
}