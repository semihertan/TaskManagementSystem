using System.ComponentModel.DataAnnotations;
using TaskManagement.API.Enums;

namespace TaskManagement.API.DTOs.User;

public class UpdateUserRoleDto
{
    [EnumDataType(typeof(UserRole), ErrorMessage = "Geçerli bir rol seçiniz.")]
    public UserRole Role { get; set; }
}
