public class UserRegisterDto {
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string UserName { get; set; }
}

public class UserLoginDto {
    public required string UserName { get; set; }
    public required string Password { get; set; }
}