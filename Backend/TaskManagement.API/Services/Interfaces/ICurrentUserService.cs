using TaskManagement.API.Enums;

namespace TaskManagement.API.Services.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    UserRole Role { get; }

    bool IsAdmin { get; }
}
