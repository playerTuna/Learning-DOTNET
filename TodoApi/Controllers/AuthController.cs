using Microsoft.AspNetCore.Mvc;

using TodoApi.Services;
using TodoApi.Exceptions;
using TodoApi.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Auth")]
public class AuthController(AuthService authService, UserService userService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
        try
        {
            await authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { message = "User registered successfully" });
        }
        catch (BadRequestException ex)
        {
            logger.LogWarning(ex, "Register rejected: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Ok(new LoginResponseDTO
            {
                AccessToken = response.AccessToken,
                ExpiresAt = response.ExpiresAt,
                RefreshToken = response.RefreshToken
            });
        }
        catch (BadRequestException ex)
        {
            logger.LogWarning(ex, "Login rejected: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rotate the access token for a user
    /// If the refresh token is valid, rotate the access token and return the new access token
    /// If the refresh token is invalid, return unauthorized
    /// </summary>
    /// <param name="refreshToken">The refresh token to rotate</param>
    /// <returns>The new access token and refresh token</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("rotate-access-token")]
    [Authorize]
    public async Task<IActionResult> RotateAccessToken([FromBody] string refreshToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }
        try
        {
            var user = await userService.GetUserById(userId);
            var newRefreshToken = await authService.RotateRefreshToken(refreshToken, userId);
            var newAccessToken = authService.GenerateAccessToken(user);
            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Rotate token: user not found for id {UserId}", userId);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning(ex, "Rotate token rejected: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }
}

