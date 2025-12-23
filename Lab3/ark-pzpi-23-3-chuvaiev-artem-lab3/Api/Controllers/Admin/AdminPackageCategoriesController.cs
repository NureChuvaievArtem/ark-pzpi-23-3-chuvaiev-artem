using Api.ApiResult;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

/// <summary>
/// CRUD operations for Package Categories
/// </summary>
[ApiController]
[Route("api/admin/package-categories")]
public class AdminPackageCategoriesController : ControllerBase
{
    private readonly IGenericRepository<PackageCategory> _packageCategoryRepository;

    public AdminPackageCategoriesController(IGenericRepository<PackageCategory> packageCategoryRepository)
    {
        _packageCategoryRepository = packageCategoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _packageCategoryRepository.GetListByConditionAsync();

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _packageCategoryRepository.GetSingleByConditionAsync(
            condition: c => c.Id == id
        );

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageCategoryDto dto)
    {
        var category = new PackageCategory { Name = dto.Name };
        var result = await _packageCategoryRepository.AddAsync(category);

        return result.Match(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePackageCategoryDto dto)
    {
        var getResult = await _packageCategoryRepository.GetSingleByConditionAsync(
            condition: c => c.Id == id
        );

        if (!getResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(getResult);
        }

        var category = getResult.Value;
        category.Name = dto.Name;
        
        var updateResult = await _packageCategoryRepository.UpdateAsync(category);

        return updateResult.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _packageCategoryRepository.DeleteAsync(x => x.Id == id);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
}

public class CreatePackageCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

