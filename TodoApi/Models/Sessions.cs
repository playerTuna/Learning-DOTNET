namespace TodoApi.Models;

public class Session
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}