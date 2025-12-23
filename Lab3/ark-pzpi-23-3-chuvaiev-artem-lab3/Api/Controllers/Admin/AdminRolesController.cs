using Api.ApiResult;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

/// <summary>
/// CRUD operations for Roles
/// </summary>
[ApiController]
[Route("api/admin/roles")]
public class AdminRolesController : ControllerBase
{
    private readonly IGenericRepository<Role> _roleRepository;

    public AdminRolesController(IGenericRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleRepository.GetListByConditionAsync();

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roleRepository.GetSingleByConditionAsync(
            condition: r => r.Id == id
        );

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var role = new Role { Name = dto.Name };
        var result = await _roleRepository.AddAsync(role);

        return result.Match(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRoleDto dto)
    {
        var getResult = await _roleRepository.GetSingleByConditionAsync(
            condition: r => r.Id == id
        );

        if (!getResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(getResult);
        }

        var role = getResult.Value;
        role.Name = dto.Name;
        
        var updateResult = await _roleRepository.UpdateAsync(role);

        return updateResult.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleRepository.DeleteAsync(x => x.Id == id);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
}

