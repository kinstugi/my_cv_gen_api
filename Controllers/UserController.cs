using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Repositories;

namespace my_cv_gen_api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
            return null;
        return userId;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var user = await _userRepository.GetUserByIdAsync(userId.Value);
        if (user is null) return NotFound();

        return Ok(ToUserResponseDto(user));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var user = await _userRepository.UpdateUserProfileAsync(userId.Value, dto);
        if (user is null) return NotFound();

        return Ok(ToUserResponseDto(user));
    }

    private static UserResponseDto ToUserResponseDto(Models.User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        IsActive = user.IsActive,
        PhoneNumber = user.PhoneNumber,
        GitHubUrl = user.GitHubUrl,
        Location = user.Location,
        Website = user.Website
    };
}
