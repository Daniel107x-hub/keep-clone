using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Reminder {
        [Key]
        public long Id { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsActive { get; set; }
    }   
}