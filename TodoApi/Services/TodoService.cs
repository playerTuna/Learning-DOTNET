namespace TodoApi.Services;
using TodoApi.Models;
using TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using TodoApi.Exceptions;

public class TodoService {
    private readonly TodoDbContext _context;

    public TodoService(TodoDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<Todo>> GetAllTasks() {
        return await _context.Todos.AsNoTracking().ToListAsync();
    }
    public async Task<Todo> GetTaskById(int id) {
        var task = await _context.Todos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id) ?? throw new NotFoundException("Task not found or deleted");
        return task;
    }

    public async Task<Todo> AddTask(Todo task) {
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            throw new BadRequestException("Title is required");
        }
        var newTask = await _context.Todos.AddAsync(new Todo {
            Title = task.Title,
            IsComplete = task.IsComplete
        });
        await _context.SaveChangesAsync();
        return newTask.Entity;
    }

    public async Task<Todo> UpdateTask(int id, Todo task) {
        if (string.IsNullOrWhiteSpace(task.Title)) {
            throw new BadRequestException("Title is required");
        }
        var existingTask = await GetTaskById(id);
        existingTask.Title = task.Title;
        existingTask.IsComplete = task.IsComplete;
        _context.Todos.Update(existingTask);
        await _context.SaveChangesAsync();
        return existingTask;
    }

    public async Task<Todo> DeleteTask(int id) {
        var task = await GetTaskById(id);
        _context.Todos.Remove(task);
        await _context.SaveChangesAsync();
        return task;
    }
}