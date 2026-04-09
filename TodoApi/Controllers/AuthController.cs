using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoApi.Services;
using TodoApi.Models;
using TodoApi.Exceptions;
using TodoApi.DTO;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "Auth")]
public class AuthController(AuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
        try
        {
            var user = await authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = user.UserId }, new { message = "User registered successfully" });
        }
        catch (BadRequestException ex)
        {
            logger.LogError(ex, "Error registering user");
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
            var token = await authService.LoginAsync(request);
            Response.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
            Response.Headers.Add("RefreshToken", token.RefreshToken);
            return Ok(new {
                ExpiredAt = token.ExpiresAt
            });
        }
        catch (BadRequestException ex)
        {
            logger.LogError(ex, "Error logging in");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed: {ErrorMessage}", ex.Message);
            return Unauthorized(new { message = "Invalid email or password" });
        }
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var token = await authService.RefreshAccessToken(refreshToken);
            return Ok(new { AccessToken = token });
        }
        catch (BadRequestException ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token: {ErrorMessage}", ex.Message);
            return Unauthorized(new { message = "Invalid refresh token" });
        }
    }
}

