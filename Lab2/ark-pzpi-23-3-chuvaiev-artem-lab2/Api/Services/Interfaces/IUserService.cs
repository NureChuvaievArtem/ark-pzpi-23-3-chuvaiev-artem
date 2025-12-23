using Api.Infrastructure.ResultPattern;
using Api.Models;
using Api.Models.DTOs;

namespace Api.Services.Interfaces;

public interface IUserService
{
    Task<Result<User>> RegisterCourierAsync(RegisterUserDto dto);
    
    Task<Result<User>> RegisterClientAsync(RegisterUserDto dto);
    
    Task<Result<User>> GetUserByIdAsync(int userId);
}