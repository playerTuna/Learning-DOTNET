namespace TodoApi.Services;

using TodoApi.Models;
using TodoApi.Data;
using TodoApi.Exceptions;
using TodoApi.DTO;
using Microsoft.EntityFrameworkCore;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

public class AuthService(TodoDbContext context, IConfiguration configuration)
{
    public readonly TodoDbContext _context = context;
    public readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Generate an access token for a user
    /// </summary>
    /// <param name="user">The user to generate an access token for</param>
    /// <returns>The access token</returns>
    /// <exception cref="BadRequestException">If the user is not found</exception>
    /// <exception cref="Exception">If the access token is not generated</exception>
    /// <exception cref="Exception">If the access token is not saved</exception>
    /// <exception cref="Exception">If the access token is not returned</exception>
    /// <exception cref="Exception">If the access token is not returned</exception>
    private string GenerateAccessToken(User user)
    {
        var secretKey = _configuration["JWT_SECRET"];
        var issuer = _configuration["JWT_ISSUER"];
        var audience = _configuration["JWT_AUDIENCE"];
        var expirationMinutes = _configuration.GetValue<int>("EXPIRES_IN_MINUTES");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.UserId)
        };
        var token = new JwtSecurityToken(issuer, audience, claims, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(expirationMinutes), credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshToken(User user)
    {
        var refreshToken = Guid.NewGuid().ToString();
        var session = new Session
        {
            UserId = user.UserId,
            RefreshToken = refreshToken
        };
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<string> RefreshAccessToken(string refreshToken)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken) ?? throw new BadRequestException("Invalid refresh token");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == session.UserId) ?? throw new BadRequestException("User not found");
        var token = GenerateAccessToken(user);
        return token;
    }

    public async Task<User> RegisterAsync(RegisterRequestDTO request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new BadRequestException("Email already exists");
        }
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new BadRequestException("Username already exists");
        }
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = passwordHash
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email) ?? throw new BadRequestException("Invalid email or password");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) throw new BadRequestException("Invalid email or password");
        var token = GenerateAccessToken(user);
        return new LoginResponseDTO {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("EXPIRES_IN_MINUTES")),
            RefreshToken = await GenerateRefreshToken(user)
        };
    }
}