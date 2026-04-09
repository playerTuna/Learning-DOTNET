using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Services;
using TodoApi.Data;
using DotNetEnv;

Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env", ".env.dev"));


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddScoped<TodoService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
