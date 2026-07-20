using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs.User;
using TaskManagement.API.Services.Interfaces;
namespace TaskManagement.API.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.API.Responses;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        CreateUserDto createUserDto)
    {
        var userExists = await _userService.UserExistsAsync(
            createUserDto.Email,
            createUserDto.Username);

        if (userExists)
        {
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message =
                    "Bu email veya kullanıcı adına kayıtlı başka bir hesap bulunmaktadır.",
                Data = null
            });
        }

        var user = await _userService.RegisterAsync(createUserDto);

        return StatusCode(
            StatusCodes.Status201Created,
            new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Kullanıcı başarıyla oluşturuldu.",
                Data = user
            });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _userService.LoginAsync(loginDto);

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Giriş başarılı.",
            Data = token
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out Guid id))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Geçersiz kullanıcı bilgisi.",
                Data = null
            });
        }

        var user = await _userService.GetProfileAsync(id);

        if (user == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Kullanıcı bulunamadı.",
                Data = null
            });
        }

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Profil başarıyla getirildi.",
            Data = user
        });
    }
}