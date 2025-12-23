using Api.Infrastructure.Errors;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Api.Models.DTOs;
using Api.Services.Interfaces;
using Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IGenericRepository<UserRole> _userRoleRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IGenericRepository<User> userRepository,
        IGenericRepository<Role> roleRepository,
        IGenericRepository<UserRole> userRoleRepository,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<User>> RegisterCourierAsync(RegisterUserDto dto)
    {
        // Validate user
        var validationResult = ValidateUserAsync(dto.EmailAddress);
        if (!validationResult.IsSuccess)
        {
            return Result<User>.Failure(validationResult.Errors);
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetSingleByConditionAsync(
            u => u.EmailAddress == dto.EmailAddress);

        if (existingUser.IsSuccess)
        {
            return Result<User>.Failure(Error.Conflict("user.ALREADY_EXISTS", "User with this email already exists"));
        }

        // Create new user
        var user = new User
        {
            EmailAddress = dto.EmailAddress,
            CreatedOn = DateTimeOffset.UtcNow,
            LastModifiedOn = DateTimeOffset.UtcNow
        };

        var addResult = await _userRepository.AddAsync(user);
        if (!addResult.IsSuccess)
        {
            return Result<User>.Failure(addResult.Errors);
        }

        // Get Courier role
        var courierRole = await _roleRepository.GetSingleByConditionAsync(r => r.Name == "Courier");
        if (!courierRole.IsSuccess)
        {
            return Result<User>.Failure(Error.NotFound("role.NOT_FOUND", "Courier role not found"));
        }

        // Assign Courier role
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = courierRole.Value.Id,
            CreatedOn = DateTimeOffset.UtcNow,
            LastModifiedOn = DateTimeOffset.UtcNow
        };

        var roleResult = await _userRoleRepository.AddAsync(userRole);
        if (!roleResult.IsSuccess)
        {
            return Result<User>.Failure(roleResult.Errors);
        }

        // Send confirmation email (non-blocking - don't fail registration if email fails)
        try
        {
            await _emailService.SendSuccessfulEmailAsync(
                user.EmailAddress,
                "You have been successfully registered as a courier.",
                "Courier Registration Successful");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send confirmation email to {Email}, but user registration succeeded", 
                user.EmailAddress);
            // Continue - don't fail registration if email fails
        }

        _logger.LogInformation("Courier registered: {Email}", dto.EmailAddress);

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> RegisterClientAsync(RegisterUserDto dto)
    {
        // Validate user
        var validationResult =  ValidateUserAsync(dto.EmailAddress);
        if (!validationResult.IsSuccess)
        {
            return Result<User>.Failure(validationResult.Errors);
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetSingleByConditionAsync(
            u => u.EmailAddress == dto.EmailAddress);

        if (existingUser.IsSuccess)
        {
            return Result<User>.Failure(Error.Conflict("user.ALREADY_EXISTS", "User with this email already exists"));
        }

        // Create new user
        var user = new User
        {
            EmailAddress = dto.EmailAddress,
            CreatedOn = DateTimeOffset.UtcNow,
            LastModifiedOn = DateTimeOffset.UtcNow
        };

        var addResult = await _userRepository.AddAsync(user);
        if (!addResult.IsSuccess)
        {
            return Result<User>.Failure(addResult.Errors);
        }

        // Get Client role
        var clientRole = await _roleRepository.GetSingleByConditionAsync(r => r.Name == "Client");
        if (!clientRole.IsSuccess)
        {
            return Result<User>.Failure(Error.NotFound("role.NOT_FOUND", "Client role not found"));
        }

        // Assign Client role
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = clientRole.Value.Id,
            CreatedOn = DateTimeOffset.UtcNow,
            LastModifiedOn = DateTimeOffset.UtcNow
        };

        var roleResult = await _userRoleRepository.AddAsync(userRole);
        if (!roleResult.IsSuccess)
        {
            return Result<User>.Failure(roleResult.Errors);
        }

        // Send confirmation email (non-blocking - don't fail registration if email fails)
        try
        {
            await _emailService.SendSuccessfulEmailAsync(
                user.EmailAddress,
                "You have been successfully registered as a client.",
                "Client Registration Successful");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send confirmation email to {Email}, but user registration succeeded", 
                user.EmailAddress);
            // Continue - don't fail registration if email fails
        }

        _logger.LogInformation("Client registered: {Email}", dto.EmailAddress);

        return Result<User>.Success(user);
    }

    private  Result<User> ValidateUserAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return Result<User>.Failure(Error.Validation("user.INVALID_EMAIL", "Invalid email address"));
        }

        return Result<User>.Success(null);
    }

    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        var result = await _userRepository.GetSingleByConditionAsync(
            u => u.EmailAddress == email);

        if (!result.IsSuccess)
        {
            return Result<User>.Failure(UserErrors.UserNotFoundError());
        }

        return Result<User>.Success(result.Value);
    }

    public async Task<Result<User>> GetUserByIdAsync(int userId)
    {
        var result = await _userRepository.GetSingleByConditionAsync(
            u => u.Id == userId);

        if (!result.IsSuccess)
        {
            return Result<User>.Failure(UserErrors.UserNotFoundError());
        }

        return Result<User>.Success(result.Value);
    }
}

