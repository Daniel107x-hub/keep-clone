using System.ComponentModel.DataAnnotations;

public class User{
    [Key]
    public long Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string UserName { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Note> Notes { get; }
}