using Microsoft.EntityFrameworkCore;
using my_cv_gen_api.Data;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Models;
using my_cv_gen_api.Services;

namespace my_cv_gen_api.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(UserLoginDto dto);
    Task<User> CreateUserAsync(UserRegisterDto dto);
    Task<User?> DeleteUserAsync(int id);
    Task<User?> LoginUserAsync(UserLoginDto dto);
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserRepository(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> GetUserByEmailAsync(UserLoginDto dto)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
    }

    public async Task<User?> LoginUserAsync(UserLoginDto dto)
    {
        var user = await GetUserByEmailAsync(dto);
        if (user is null) return null;
        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt)) return null;
        return user;
    }

    public async Task<User> CreateUserAsync(UserRegisterDto dto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var (hash, salt) = _passwordHasher.HashPassword(dto.Password);
        var now = DateTime.UtcNow;

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return null;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return user;
    }
}
