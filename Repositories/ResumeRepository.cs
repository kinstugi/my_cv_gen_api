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
    Task<Resume?> GetResumeByIdForOwnerAsync(int id, int userId);
    Task<Resume> UpdateResumeAsync(int id, ResumeUpdateDto dto, int userId);
    Task<Resume?> DeleteResumeAsync(int id, int userId);
    Task<List<Resume>> GetResumesByUserIdAsync(int userId, int page, int pageSize);
}

public class ResumeRepository : IResumeRepository
{
    private readonly AppDbContext _context;
    public ResumeRepository(AppDbContext context)
    {
        _context = context;
    }

    private static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) // Unspecified - assume UTC
        };
    }

    private static DateTime? ToUtc(DateTime? dateTime)
    {
        return dateTime.HasValue ? ToUtc(dateTime.Value) : null;
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
        
        // Add WorkExperiences
        foreach (var weDto in dto.WorkExperiences)
        {
            var bullets = (weDto.Description ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            resume.WorkExperiences.Add(new WorkExperience
            {
                Company = weDto.Company,
                Position = weDto.Position,
                Description = bullets,
                StartDate = ToUtc(weDto.StartDate),
                EndDate = ToUtc(weDto.EndDate),
                IsCurrent = weDto.IsCurrent,
                Resume = resume
            });
        }
        
        // Add Educations
        foreach (var eduDto in dto.Educations)
        {
            resume.Educations.Add(new Education
            {
                School = eduDto.School,
                Degree = eduDto.Degree,
                FieldOfStudy = eduDto.FieldOfStudy,
                StartDate = ToUtc(eduDto.StartDate),
                EndDate = ToUtc(eduDto.EndDate),
                Resume = resume
            });
        }
        
        // Add Languages
        foreach (var langDto in dto.Languages)
        {
            resume.Languages.Add(new Language
            {
                Name = langDto.Name,
                Level = langDto.Level,
                Resume = resume
            });
        }
        
        // Add Projects
        foreach (var projDto in dto.Projects)
        {
            resume.Projects.Add(new Project
            {
                Title = projDto.Title,
                Description = projDto.Description,
                Link = projDto.Link,
                CreatedAt = now,
                UpdatedAt = now,
                Resume = resume
            });
        }
        
        // Add Skills
        resume.Skills = dto.Skills.ToList();
        
        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();
        
        // Reload with includes to return complete data
        return await _context.Resumes
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Languages)
            .Include(r => r.Projects)
            .FirstAsync(r => r.Id == resume.Id);
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

    public async Task<Resume?> GetResumeByIdForOwnerAsync(int id, int userId)
    {
        return await _context.Resumes
            .Where(r => r.Id == id && r.UserId == userId && r.IsActive)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Languages)
            .Include(r => r.Projects)
            .Include(r => r.User)
            .FirstOrDefaultAsync();
    }

    public async Task<Resume> UpdateResumeAsync(int id, ResumeUpdateDto dto, int userId)
    {
        var resume = await _context.Resumes
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Languages)
            .Include(r => r.Projects)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (resume is null || resume.UserId != userId)
        {
            throw new NotFoundException("Resume not found");
        }
        
        // Update top-level fields
        resume.Title = dto.Title;
        resume.Description = dto.Description;
        resume.ImageUrl = dto.ImageUrl;
        if (dto.IsActive.HasValue)
            resume.IsActive = dto.IsActive.Value;
        resume.UpdatedAt = DateTime.UtcNow;
        
        var now = DateTime.UtcNow;
        
        // Update WorkExperiences if provided
        if (dto.WorkExperiences is not null)
        {
            _context.WorkExperiences.RemoveRange(resume.WorkExperiences);
            resume.WorkExperiences.Clear();
            foreach (var weDto in dto.WorkExperiences)
            {
                var bullets = (weDto.Description ?? string.Empty)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                resume.WorkExperiences.Add(new WorkExperience
                {
                    Company = weDto.Company,
                    Position = weDto.Position,
                    Description = bullets,
                    StartDate = ToUtc(weDto.StartDate),
                    EndDate = ToUtc(weDto.EndDate),
                    IsCurrent = weDto.IsCurrent,
                    Resume = resume
                });
            }
        }
        
        // Update Educations if provided
        if (dto.Educations is not null)
        {
            _context.Educations.RemoveRange(resume.Educations);
            resume.Educations.Clear();
            foreach (var eduDto in dto.Educations)
            {
                resume.Educations.Add(new Education
                {
                    School = eduDto.School,
                    Degree = eduDto.Degree,
                    FieldOfStudy = eduDto.FieldOfStudy,
                    StartDate = ToUtc(eduDto.StartDate),
                    EndDate = ToUtc(eduDto.EndDate),
                    Resume = resume
                });
            }
        }
        
        // Update Languages if provided
        if (dto.Languages is not null)
        {
            _context.Languages.RemoveRange(resume.Languages);
            resume.Languages.Clear();
            foreach (var langDto in dto.Languages)
            {
                resume.Languages.Add(new Language
                {
                    Name = langDto.Name,
                    Level = langDto.Level,
                    Resume = resume
                });
            }
        }
        
        // Update Projects if provided
        if (dto.Projects is not null)
        {
            _context.Projects.RemoveRange(resume.Projects);
            resume.Projects.Clear();
            foreach (var projDto in dto.Projects)
            {
                resume.Projects.Add(new Project
                {
                    Title = projDto.Title,
                    Description = projDto.Description,
                    Link = projDto.Link,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Resume = resume
                });
            }
        }
        
        // Update Skills if provided
        if (dto.Skills is not null)
        {
            resume.Skills = dto.Skills.ToList();
        }
        
        await _context.SaveChangesAsync();
        
        // Reload with includes to return complete data
        return await _context.Resumes
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Languages)
            .Include(r => r.Projects)
            .FirstAsync(r => r.Id == resume.Id);
    }

    public async Task<Resume?> DeleteResumeAsync(int id, int userId)
    {
        var resume = await _context.Resumes.FindAsync(id);
        if (resume is null || resume.UserId != userId)
            return null;
        resume.IsActive = false;
        resume.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return resume;
    }

    public async Task<List<Resume>> GetResumesByUserIdAsync(int userId, int page, int pageSize)
    {
        return await _context.Resumes
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.UpdatedAt)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Languages)
            .Include(r => r.Projects)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}