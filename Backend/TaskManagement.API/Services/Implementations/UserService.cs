using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.User;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<UserDto> RegisterAsync(CreateUserDto createUserDto)
    {
        // kayıtlı var mı kontrolü
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == createUserDto.Email || x.Username == createUserDto.Username);
        
        if(existingUser != null)
        {
            throw new Exception("Bu Email'e kayıtlı başka bir hesap bulunmaktadır!");
        }
         
        // entity oluşturma
        var user = _mapper.Map<User>(createUserDto);
        // şifre hashle
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        // kaydet
        await _context.Users.AddAsync(user);
        await _context.Users.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
    public Task<UserDto?> GetProfileAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
            loginDto.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new Exception("Email veya şifre hatalı");
        }

        return string.Empty;
    }
}