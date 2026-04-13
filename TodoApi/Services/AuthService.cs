namespace TodoApi.Services;

using Models;
using Data;
using Exceptions;
using DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

public class AuthService(TodoDbContext context, IConfiguration configuration)
{
    public readonly TodoDbContext Context = context;
    public readonly IConfiguration Configuration = configuration;

    /// <summary>
    /// Generate an access token for a user
    /// </summary>
    /// <param name="user">The user to generate an access token for</param>
    /// <returns>The access token</returns>
    public string GenerateAccessToken(User user)
    {
        var secretKey = Configuration["JWT_SECRET"];
        var issuer = Configuration["JWT_ISSUER"];
        var audience = Configuration["JWT_AUDIENCE"];
        var expirationMinutes = Configuration.GetValue<int>("EXPIRES_IN_MINUTES");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.UserId)
        };
        var token = new JwtSecurityToken(issuer, audience, claims, DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(expirationMinutes), credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate a refresh token for a user
    /// </summary>
    /// Query Refresh Token from the database, then check if it is expired or revoked.
    /// If it is revoked or expired, delete it from the database then throw bad request exception
    /// <param name="refreshToken">The refresh token to rotate</param>
    /// <param name="userId">The user id to check if the refresh token is valid for</param>
    /// <returns>The new refresh token</returns>
    /// <exception cref="UnauthorizedException">If the refresh token is invalid, wrong user, revoked, or expired</exception>
    public async Task<string> RotateRefreshToken(string refreshToken, string userId)
    {
        var session = await Context.Sessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken) ??
                      throw new UnauthorizedException("Invalid refresh token");
        if (session.UserId != userId)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (session.RevokedAt != null)
        {
            Context.Sessions.Remove(session);
            await Context.SaveChangesAsync();
            throw new UnauthorizedException("Please login again");
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            session.RevokedAt = DateTime.UtcNow;
            Context.Sessions.Remove(session);
            await Context.SaveChangesAsync();
            throw new UnauthorizedException("Please login again");
        }

        session.ExpiresAt = DateTime.UtcNow.AddMinutes(43200);
        session.RefreshToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        await Context.SaveChangesAsync();
        return session.RefreshToken;
    }

    /// <summary>
    /// Login a user
    /// </summary>
    /// <param name="request">The login request</param>
    /// <returns>The login response</returns>
    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await Context.Users.FirstOrDefaultAsync(u => u.Email == request.Email) ??
                   throw new BadRequestException("Invalid email or password");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new BadRequestException("Invalid email or password");
        var token = GenerateAccessToken(user);
        var refreshToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var session = new Session
        {
            UserId = user.UserId,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(43200),
        };
        await Context.Sessions.AddAsync(session);
        await Context.SaveChangesAsync();
        return new LoginResponseDTO
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(Configuration.GetValue<int>("EXPIRES_IN_MINUTES")),
            RefreshToken = refreshToken,
        };
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">The register request</param>
    /// <returns>The registered user</returns>
    public async Task<User> RegisterAsync(RegisterRequestDTO request)
    {
        if (await Context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new BadRequestException("Email already exists");
        }

        if (await Context.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new BadRequestException("Username already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = passwordHash,
        };
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        return user;
    }
}