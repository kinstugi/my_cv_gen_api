using Microsoft.EntityFrameworkCore;
using my_cv_gen_api.Data;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Models;
using my_cv_gen_api.Services;

namespace my_cv_gen_api.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(UserLoginDto dto);
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(UserRegisterDto dto);
    Task<User?> UpdateUserProfileAsync(int id, UserProfileUpdateDto dto);
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
            IsActive = true,
            PhoneNumber = dto.PhoneNumber,
            GitHubUrl = dto.GitHubUrl,
            Location = dto.Location,
            Website = dto.Website
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> UpdateUserProfileAsync(int id, UserProfileUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return null;

        // Only update fields that are explicitly provided (non-null in DTO)
        if (dto.FirstName is not null) user.FirstName = dto.FirstName;
        if (dto.LastName is not null) user.LastName = dto.LastName;
        // Empty string means "clear" for optional profile fields
        if (dto.PhoneNumber is not null) user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
        if (dto.GitHubUrl is not null) user.GitHubUrl = string.IsNullOrWhiteSpace(dto.GitHubUrl) ? null : dto.GitHubUrl;
        if (dto.Location is not null) user.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location;
        if (dto.Website is not null) user.Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website;

        user.UpdatedAt = DateTime.UtcNow;
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
