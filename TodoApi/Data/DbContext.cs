namespace TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using Models;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<DbContext> options) : base(options) { }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
}