using TaskManagement.API.DTOs.User;
using TaskManagement.API.Enums;

namespace TaskManagement.API.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(CreateUserDto createUserDto);

    Task<string> LoginAsync(LoginDto loginDto);

    Task<UserDto> GetProfileAsync(Guid userId);

    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserDto updateUserDto);

    Task ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

    Task<bool> UserExistsAsync(string email, string username);

    Task<IReadOnlyList<UserDto>> GetAllUsersAsync();

    Task<UserDto> GetUserByIdAsync(Guid userId);

    Task<UserDto> UpdateUserRoleAsync(Guid userId, UserRole role);

    Task<UserDto> UpdateUserStatusAsync(Guid userId, bool isActive);
}
