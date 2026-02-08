using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Exceptions;
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

    [HttpPost]
    public async Task<IActionResult> CreateResume([FromBody] ResumeCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var resume = await _resumeRepository.CreateResumeAsync(dto, userId.Value);
        return Ok(resume);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetResumeById(int id)
    {
        var resume = await _resumeRepository.GetResumeByIdAsync(id);
        if (resume is null)
            return NotFound();
        return Ok(resume);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResume(int id, [FromBody] ResumeUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var resume = await _resumeRepository.UpdateResumeAsync(id, dto, userId.Value);
            return Ok(resume);
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
        return Ok(resume);
    }

    [HttpGet]
    public async Task<IActionResult> GetResumes(int page = 1, int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var resumes = await _resumeRepository.GetResumesByUserIdAsync(userId.Value, page, pageSize);
        return Ok(resumes);
    }
}