using TaskManagement.API.DTOs.User;

namespace TaskManagement.API.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(CreateUserDto createUserDto);

    Task<string> LoginAsync(LoginDto loginDto);

    Task<UserDto?> GetProfileAsync(Guid userId);
}