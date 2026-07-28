using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs.User;
using TaskManagement.API.Responses;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Controllers;

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
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var user = await _userService.GetProfileAsync(GetAuthenticatedUserId());

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Profil başarıyla getirildi.",
            Data = user
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await _userService.UpdateProfileAsync(
            GetAuthenticatedUserId(),
            updateUserDto);

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Profil bilgileriniz güncellendi.",
            Data = user
        });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordDto changePasswordDto)
    {
        await _userService.ChangePasswordAsync(
            GetAuthenticatedUserId(),
            changePasswordDto);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Şifreniz başarıyla değiştirildi.",
            Data = null
        });
    }

    private Guid GetAuthenticatedUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException("Geçersiz kullanıcı bilgisi.");
        }

        return id;
    }
}
