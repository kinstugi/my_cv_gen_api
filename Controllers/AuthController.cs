using Microsoft.AspNetCore.Mvc;
using my_cv_gen_api.DTOs;
using my_cv_gen_api.Repositories;

namespace my_cv_gen_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        var user = await _userRepository.LoginUserAsync(userLoginDto);
        if (user is null) return Unauthorized();
        return Ok(ToUserResponseDto(user));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        var user = await _userRepository.CreateUserAsync(userRegisterDto);
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
        IsActive = user.IsActive
    };
}