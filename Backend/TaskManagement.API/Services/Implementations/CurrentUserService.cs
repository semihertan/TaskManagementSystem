using System.Security.Claims;
using TaskManagement.API.Enums;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdValue = Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Geçersiz kullanıcı bilgisi.");
            }

            return userId;
        }
    }

    public UserRole Role
    {
        get
        {
            var roleValue = Principal.FindFirstValue(ClaimTypes.Role);

            return Enum.TryParse<UserRole>(roleValue, true, out var role)
                ? role
                : UserRole.User;
        }
    }

    public bool IsAdmin => Role == UserRole.Admin;

    private ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("Kullanıcı bilgisi bulunamadı.");
}
