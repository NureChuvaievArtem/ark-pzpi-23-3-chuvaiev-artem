using Api.Infrastructure.Errors;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class NfcService : INfcService
{
    private readonly IGenericRepository<User> _userRepository;

    public NfcService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<string>> RegisterNfcToUserAsync(int userId, string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return Result<string>.Failure(Error.Validation("nfc.INVALID_SERIAL", "Serial number cannot be empty"));
        }

        // Check if this serial number is already registered to another user
        var existingUserBySerial = await _userRepository.GetSingleByConditionAsync(
            u => u.SerialNfcData == serialNumber);

        if (existingUserBySerial.IsSuccess && existingUserBySerial.Value.Id != userId)
        {
            return Result<string>.Failure(NfcErrors.NfcAlreadyRegistered());
        }

        // Get the user
        var userResult = await _userRepository.GetSingleByConditionAsync(u => u.Id == userId);
        if (!userResult.IsSuccess)
        {
            return Result<string>.Failure(UserErrors.UserNotFoundError());
        }

        var user = userResult.Value;

        // If user already has the same serial number, just return success
        if (user.SerialNfcData == serialNumber)
        {
            return Result<string>.Success(serialNumber);
        }

        // Update user's SerialNfcData (replaces any existing NFC)
        user.SerialNfcData = serialNumber;
        user.LastModifiedOn = DateTimeOffset.UtcNow;

        var updateResult = await _userRepository.UpdateAsync(user);
        if (!updateResult.IsSuccess)
        {
            return Result<string>.Failure(updateResult.Errors);
        }

        return Result<string>.Success(serialNumber);
    }

    public async Task<Result<string?>> GetNfcSerialByUserIdAsync(int userId)
    {
        var userResult = await _userRepository.GetSingleByConditionAsync(u => u.Id == userId);
        
        if (!userResult.IsSuccess)
        {
            return Result<string?>.Failure(UserErrors.UserNotFoundError());
        }

        return Result<string?>.Success(userResult.Value.SerialNfcData);
    }

    public async Task<Result<int?>> GetUserIdByNfcSerialAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return Result<int?>.Failure(Error.Validation("nfc.INVALID_SERIAL", "Serial number cannot be empty"));
        }

        var userResult = await _userRepository.GetSingleByConditionAsync(
            u => u.SerialNfcData == serialNumber);

        if (!userResult.IsSuccess)
        {
            return Result<int?>.Failure(NfcErrors.NfcNotFound());
        }

        return Result<int?>.Success(userResult.Value.Id);
    }

    public async Task<Result<IEnumerable<User>>> GetAllUsersWithNfcAsync()
    {
        var result = await _userRepository.GetListByConditionAsync(
            u => u.SerialNfcData != null && !string.IsNullOrWhiteSpace(u.SerialNfcData));

        if (!result.IsSuccess)
        {
            return Result<IEnumerable<User>>.Failure(result.Errors);
        }

        return Result<IEnumerable<User>>.Success(result.Value);
    }
    
    public async Task<Result<User>> ValidateNfcCardAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return Result<User>.Failure(NfcErrors.NfcNotFound());
        }

        var userResult = await _userRepository.GetSingleByConditionAsync(
            u => u.SerialNfcData == serialNumber, includes: [u => u.Include(s => s.Roles).ThenInclude(s => s.Role)]);

        if (!userResult.IsSuccess)
        {
            return Result<User>.Failure(NfcErrors.NfcNotFound());
        }

        return Result<User>.Success(userResult.Value);
    }
}