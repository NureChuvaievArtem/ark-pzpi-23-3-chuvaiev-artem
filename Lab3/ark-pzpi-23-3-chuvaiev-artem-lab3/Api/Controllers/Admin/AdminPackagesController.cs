using Api.ApiResult;
using Api.Infrastructure.Repository;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

/// <summary>
/// CRUD operations for Packages
/// </summary>
[ApiController]
[Route("api/admin/packages")]
public class AdminPackagesController : ControllerBase
{
    private readonly IGenericRepository<Package> _packageRepository;

    public AdminPackagesController(IGenericRepository<Package> packageRepository)
    {
        _packageRepository = packageRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _packageRepository.GetListByConditionAsync(
            includes: new List<Func<IQueryable<Package>, IQueryable<Package>>>
            {
                q => q.Include(p => p.User),
                q => q.Include(p => p.Category),
                q => q.Include(p => p.DeliveryStatus)
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
        var result = await _packageRepository.GetSingleByConditionAsync(
            condition: p => p.Id == id,
            includes: new List<Func<IQueryable<Package>, IQueryable<Package>>>
            {
                q => q.Include(p => p.User),
                q => q.Include(p => p.Category),
                q => q.Include(p => p.DeliveryStatus)
            }
        );

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePackageDto dto)
    {
        var getResult = await _packageRepository.GetSingleByConditionAsync(
            condition: p => p.Id == id
        );

        if (!getResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(getResult);
        }

        var package = getResult.Value;
        
        if (dto.Height.HasValue) package.Height = dto.Height.Value;
        if (dto.Width.HasValue) package.Width = dto.Width.Value;
        if (dto.Depth.HasValue) package.Depth = dto.Depth.Value;
        if (dto.PostBoxId.HasValue) package.PostBoxId = dto.PostBoxId.Value;
        
        var updateResult = await _packageRepository.UpdateAsync(package);

        return updateResult.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _packageRepository.DeleteAsync(x => x.Id == id);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
}

public class UpdatePackageDto
{
    public int? Height { get; set; }
    public int? Width { get; set; }
    public int? Depth { get; set; }
    public int? PostBoxId { get; set; }
}

