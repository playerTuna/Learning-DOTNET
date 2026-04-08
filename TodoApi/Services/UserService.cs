namespace TodoApi.Services;
using TodoApi.Models;
using TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using TodoApi.Exceptions;

public class UserService
{
    private readonly TodoDbContext _context;

    public UserService(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetUserById(string userId)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId) ?? throw new NotFoundException("User not found");
    }

    public async Task<User> CreateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
        {
            throw new BadRequestException("Username, Email, and Password are required");
        }
        var newUser = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return newUser.Entity;
    }

    public async Task<User> UpdateUser(string userId, User user)
    {
        var existingUser = await GetUserById(userId);
        existingUser.Username = user.Username;
        existingUser.Email = user.Email;
        existingUser.Password = user.Password;
        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<User> DeleteUser(string userId)
    {
        var user = await GetUserById(userId);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return user;
    }
}