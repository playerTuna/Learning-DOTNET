namespace TodoApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Services;
using TodoApi.Models;
using TodoApi.Exceptions;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserService userService, ILogger<UserController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserById(string id)
    {
        try
        {
            var user = await userService.GetUserById(id);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        try
        {
            var newUser = await userService.CreateUser(user);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, newUser);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> UpdateUser(string id, User user)
    {
        try
        {
            var updatedUser = await userService.UpdateUser(id, user);
            return Ok(updatedUser);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> DeleteUser(string id)
    {
        try
        {
            var deletedUser = await userService.DeleteUser(id);
            return Ok(deletedUser);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}