using Api.Infrastructure.ResultPattern;
using Api.Models;

namespace Api.Services.Interfaces;

public interface INfcService
{
    Task<Result<string>> RegisterNfcToUserAsync(int userId, string serialNumber);
    
    Task<Result<string?>> GetNfcSerialByUserIdAsync(int userId);
    
    Task<Result<int?>> GetUserIdByNfcSerialAsync(string serialNumber);
    
    Task<Result<IEnumerable<User>>> GetAllUsersWithNfcAsync();
    
    Task<Result<User>> ValidateNfcCardAsync(string serialNumber);
}