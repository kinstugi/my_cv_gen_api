using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Exceptions;
using my_cv_gen_api.Models;
using my_cv_gen_api.Repositories;

namespace my_cv_gen_api.Controllers;

[ApiController]
[Authorize]
[Route("api/resumes")]
public class ResumeController : ControllerBase
{
    private readonly IResumeRepository _resumeRepository;

    public ResumeController(IResumeRepository resumeRepository)
    {
        _resumeRepository = resumeRepository;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
            return null;
        return userId;
    }

    private static ResumeResponseDto ToResumeResponseDto(Resume r)
    {
        return new ResumeResponseDto
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            ImageUrl = r.ImageUrl,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            WorkExperiences = r.WorkExperiences.Select(w => new WorkExperienceResponseDto
            {
                Id = w.Id,
                Company = w.Company,
                Position = w.Position,
                Description = w.Description,
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                IsCurrent = w.IsCurrent
            }).ToList(),
            Educations = r.Educations.Select(e => new EducationResponseDto
            {
                Id = e.Id,
                School = e.School,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList(),
            Languages = r.Languages.Select(l => new LanguageResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Level = l.Level
            }).ToList(),
            Projects = r.Projects.Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Link = p.Link
            }).ToList(),
            Skills = r.Skills.ToList()
        };
    }

    [HttpPost]
    public async Task<IActionResult> CreateResume([FromBody] ResumeCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var resume = await _resumeRepository.CreateResumeAsync(dto, userId.Value);
        return Ok(ToResumeResponseDto(resume));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetResumeById(int id)
    {
        var resume = await _resumeRepository.GetResumeByIdAsync(id);
        if (resume is null)
            return NotFound();
        return Ok(ToResumeResponseDto(resume));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResume(int id, [FromBody] ResumeUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var resume = await _resumeRepository.UpdateResumeAsync(id, dto, userId.Value);
            return Ok(ToResumeResponseDto(resume));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResume(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var resume = await _resumeRepository.DeleteResumeAsync(id, userId.Value);
        if (resume is null)
            return NotFound();
        return Ok(ToResumeResponseDto(resume));
    }

    [HttpGet]
    public async Task<IActionResult> GetResumes(int page = 1, int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var resumes = await _resumeRepository.GetResumesByUserIdAsync(userId.Value, page, pageSize);
        return Ok(resumes.Select(ToResumeResponseDto).ToList());
    }
}