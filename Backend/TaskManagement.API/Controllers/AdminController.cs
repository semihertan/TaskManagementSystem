using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs.User;
using TaskManagement.API.Enums;
using TaskManagement.API.Responses;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();

        return Ok(new ApiResponse<IReadOnlyList<UserDto>>
        {
            Success = true,
            Message = "Kullanıcılar başarıyla getirildi.",
            Data = users
        });
    }

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Kullanıcı başarıyla getirildi.",
            Data = user
        });
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleDto request)
    {
        var user = await _userService.UpdateUserRoleAsync(id, request.Role);

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Kullanıcı rolü güncellendi. Yeni rol için kullanıcının yeniden giriş yapması gerekir.",
            Data = user
        });
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateUserStatusDto request)
    {
        var user = await _userService.UpdateUserStatusAsync(id, request.IsActive);

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = request.IsActive
                ? "Kullanıcı aktifleştirildi."
                : "Kullanıcı pasifleştirildi.",
            Data = user
        });
    }
}
