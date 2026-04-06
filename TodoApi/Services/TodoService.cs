namespace TodoApi.Services;
using TodoApi.Models;

public class TodoService {
    private readonly List<Todo> _todos = new List<Todo>();
    private int _taskId = 1;

    public IEnumerable<Todo> GetAllTasks() {
        return _todos;
    }

    public Todo? GetTaskById(int id) {
        return _todos.FirstOrDefault(t => t.Id == id);
    }

    public void AddTask(Todo task) {
        task.Id = _taskId++;
        task.Title = task.Title.Trim();
        if (string.IsNullOrEmpty(task.Title)) {
            throw new ArgumentException("Title is required");
        }
        // abcxyz
        _todos.Add(task);
    }

    public void UpdateTask(Todo task) {
        var existingTask = GetTaskById(task.Id);
        if (existingTask is not null) {
            existingTask.Title = task.Title;
            existingTask.IsComplete = task.IsComplete;
        }
    }

    public void DeleteTask(int id) {
        var task = GetTaskById(id);
        if (task is not null) {
            _todos.Remove(task);
        }
    }
}