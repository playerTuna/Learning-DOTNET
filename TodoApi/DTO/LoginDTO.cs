using System.ComponentModel.DataAnnotations;
namespace TodoApi.DTO;

public class LoginRequestDTO {
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(20)]
    public required string Password { get; set; }
}

public class LoginResponseDTO {
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}