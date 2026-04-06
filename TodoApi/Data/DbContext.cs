namespace TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

public class TodoDbContext : DbContext {
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}
    public DbSet<Todo> Todos { get; set; }

    public int test() {}

    // 1 hai ba 4 5 7
}

// Chao Phat Nguyen
// Chao Tien Nam