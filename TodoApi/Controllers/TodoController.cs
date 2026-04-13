namespace TodoApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using Models;
using Exceptions;

[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly TodoService _todoService;

    public TodoController(TodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Todo>>> GetAllTasks()
    {
        var tasks = await _todoService.GetAllTasks();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Todo>> GetTaskById(int id)
    {
        try
        {
            var task = await _todoService.GetTaskById(id);
            return Ok(task);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Todo>> AddTask(Todo task)
    {
        try
        {
            var newTask = await _todoService.AddTask(task);
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Todo>> UpdateTask(int id, Todo task)
    {
        try
        {
            var updatedTask = await _todoService.UpdateTask(id, task);
            return Ok(updatedTask);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Todo>> DeleteTask(int id)
    {
        try
        {
            var deletedTask = await _todoService.DeleteTask(id);
            return Ok(deletedTask);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}