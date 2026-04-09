namespace TodoApi.Models;

public class User
{
    public string UserId { get; set; } = Guid.NewGuid().ToString();
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}