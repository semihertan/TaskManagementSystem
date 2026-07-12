using AutoMapper;
using TaskManagement.API.DTOs.Category;
using TaskManagement.API.DTOs.Task;
using TaskManagement.API.DTOs.User;
using TaskManagement.API.Entities;

namespace TaskManagement.API.Mappings;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();

        // Task
        CreateMap<TaskItem, TaskItemDto>();
        CreateMap<CreateTaskDto, TaskItem>();
        CreateMap<UpdateTaskDto, TaskItem>();

        // Category
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();
    }
}