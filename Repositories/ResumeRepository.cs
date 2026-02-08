using Microsoft.EntityFrameworkCore;
using my_cv_gen_api.Data;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Exceptions;
using my_cv_gen_api.Models;

namespace my_cv_gen_api.Repositories;

public interface IResumeRepository
{
    Task<Resume> CreateResumeAsync(ResumeCreateDto dto, int userId);
    Task<Resume?> GetResumeByIdAsync(int id);
    Task<Resume> UpdateResumeAsync(int id, ResumeUpdateDto dto);
    Task<Resume?> DeleteResumeAsync(int id);
}

public class ResumeRepository : IResumeRepository
{
    private readonly AppDbContext _context;
    public ResumeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Resume> CreateResumeAsync(ResumeCreateDto dto, int userId)
    {
        var now = DateTime.UtcNow;
        var resume = new Resume
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();
        return resume;
    }

    public async Task<Resume?> GetResumeByIdAsync(int id)
    {
        return await _context.Resumes
            .Where(r => r.Id == id && r.IsActive)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Languages)
            .Include(r => r.Projects)
            .FirstOrDefaultAsync();
    }

    public async Task<Resume> UpdateResumeAsync(int id, ResumeUpdateDto dto)
    {
        var resume = await _context.Resumes.FindAsync(id);
        if (resume is null)
        {
            throw new NotFoundException("Resume not found");
        }
        resume.Title = dto.Title;
        resume.Description = dto.Description;
        resume.ImageUrl = dto.ImageUrl;
        if (dto.IsActive.HasValue)
            resume.IsActive = dto.IsActive.Value;
        resume.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return resume;
    }

    public async Task<Resume?> DeleteResumeAsync(int id)
    {
        var resume = await _context.Resumes.FindAsync(id);
        if (resume is null) return null;
        resume.IsActive = false;
        resume.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return resume;
    }
}