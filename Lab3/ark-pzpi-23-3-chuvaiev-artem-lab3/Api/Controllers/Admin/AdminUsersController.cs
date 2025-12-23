using Api.ApiResult;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

/// <summary>
/// CRUD operations for Users
/// </summary>
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IGenericRepository<User> _userRepository;

    public AdminUsersController(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userRepository.GetListByConditionAsync(
            includes: new List<Func<IQueryable<User>, IQueryable<User>>>
            {
                q => q.Include(u => u.Roles),
                q => q.Include(u => u.Packages)
            }
        );

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userRepository.GetSingleByConditionAsync(
            condition: u => u.Id == id,
            includes: new List<Func<IQueryable<User>, IQueryable<User>>>
            {
                q => q.Include(u => u.Roles),
                q => q.Include(u => u.Packages)
            }
        );

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = new User
        {
            EmailAddress = dto.EmailAddress,
            SerialNfcData = dto.SerialNfcData
        };
        
        var result = await _userRepository.AddAsync(user);

        return result.Match(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var getResult = await _userRepository.GetSingleByConditionAsync(
            condition: u => u.Id == id
        );

        if (!getResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(getResult);
        }

        var user = getResult.Value;
        user.EmailAddress = dto.EmailAddress ?? user.EmailAddress;
        user.SerialNfcData = dto.SerialNfcData ?? user.SerialNfcData;
        
        var updateResult = await _userRepository.UpdateAsync(user);

        return updateResult.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userRepository.DeleteAsync(x => x.Id == id);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
}

public class CreateUserDto
{
    public string EmailAddress { get; set; } = string.Empty;
    public string? SerialNfcData { get; set; }
}

public class UpdateUserDto
{
    public string? EmailAddress { get; set; }
    public string? SerialNfcData { get; set; }
}

