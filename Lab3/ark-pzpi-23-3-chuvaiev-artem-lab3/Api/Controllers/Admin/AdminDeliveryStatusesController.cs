using Api.ApiResult;
using Api.Infrastructure.Repository;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

/// <summary>
/// CRUD operations for Delivery Statuses
/// </summary>
[ApiController]
[Route("api/admin/delivery-statuses")]
public class AdminDeliveryStatusesController : ControllerBase
{
    private readonly IGenericRepository<DeliveryStatus> _deliveryStatusRepository;

    public AdminDeliveryStatusesController(IGenericRepository<DeliveryStatus> deliveryStatusRepository)
    {
        _deliveryStatusRepository = deliveryStatusRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _deliveryStatusRepository.GetListByConditionAsync();

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _deliveryStatusRepository.GetSingleByConditionAsync(
            condition: s => s.Id == id
        );

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryStatusDto dto)
    {
        var status = new DeliveryStatus { Name = dto.Name };
        var result = await _deliveryStatusRepository.AddAsync(status);

        return result.Match(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateDeliveryStatusDto dto)
    {
        var getResult = await _deliveryStatusRepository.GetSingleByConditionAsync(
            condition: s => s.Id == id
        );

        if (!getResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(getResult);
        }

        var status = getResult.Value;
        status.Name = dto.Name;
        
        var updateResult = await _deliveryStatusRepository.UpdateAsync(status);

        return updateResult.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _deliveryStatusRepository.DeleteAsync(x => x.Id == id);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
}

public class CreateDeliveryStatusDto
{
    public string Name { get; set; } = string.Empty;
}

