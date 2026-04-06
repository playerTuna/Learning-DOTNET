namespace TodoApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Services;
using TodoApi.Models;

[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase {
    private readonly TodoService _todoService;

    public TodoController(TodoService todoService) {
        _todoService = todoService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Todo>> GetAllTasks() {
        return Ok(_todoService.GetAllTasks());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Todo> GetTaskById(int id) {
        var task = _todoService.GetTaskById(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<Todo> AddTask(Todo task) {
        _todoService.AddTask(task);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult UpdateTask(int id, Todo task) {
        if (id != task.Id) {
            return BadRequest();
        }
        var existingTask = _todoService.GetTaskById(id);
        if (existingTask is null) {
            return NotFound();
        }
        _todoService.UpdateTask(task);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult DeleteTask(int id) {
        var task = _todoService.GetTaskById(id);
        if (task is null) {
            return NotFound();
        }
        _todoService.DeleteTask(id);
        return NoContent();
    }
}