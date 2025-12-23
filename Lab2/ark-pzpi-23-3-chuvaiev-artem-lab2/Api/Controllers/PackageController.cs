using Api.ApiResult;
using Api.Infrastructure.Errors;
using Api.Models.DTOs;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackageController : ControllerBase
{
    private readonly IPackageService _packageService;
    private readonly INfcService _nfcService;
    private readonly ILogger<PackageController> _logger;

    public PackageController(
        IPackageService packageService,
        INfcService nfcValidationService,
        ILogger<PackageController> logger)
    {
        _packageService = packageService;
        _nfcService = nfcValidationService;
        _logger = logger;
    }

    [HttpGet("courier")]
    public async Task<IActionResult> GetPackagesForCourier([FromQuery] string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return ApiResults.ToProblemDetails(NfcErrors.NfcNotFound());
        }

        var validationResult = await _nfcService.ValidateNfcCardAsync(serialNumber);
        if (!validationResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(validationResult.Errors.First());
        }

        _logger.LogInformation("Get packages for courier with NFC: {SerialNumber}", serialNumber);

        var result = await _packageService.GetPackagesForCourierAsync(serialNumber);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("client/{userId}")]
    public async Task<IActionResult> GetPackagesForClient(int userId)
    {
        _logger.LogInformation("Get packages for client: {UserId}", userId);

        var result = await _packageService.GetPackagesForClientAsync(userId);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("{packageId}/status/in-progress")]
    public async Task<IActionResult> ChangeStatusToInProgress(int packageId, [FromQuery] string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return ApiResults.ToProblemDetails(NfcErrors.NfcNotFound());
        }

        var validationResult = await _nfcService.ValidateNfcCardAsync(serialNumber);
        if (!validationResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(validationResult.Errors.First());
        }

        _logger.LogInformation("Change package {PackageId} status to InProgress", packageId);

        var result = await _packageService.ChangePackageStatusToInProgressAsync(packageId, serialNumber);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("courier/locker/open-for-placement")]
    public async Task<IActionResult> OpenLockerForPlacement([FromBody] OpenLockerDto dto)
    {
        // Validate NFC card
        var validationResult = await _nfcService.ValidateNfcCardAsync(dto.SerialNumber);
        if (!validationResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(validationResult.Errors.First());
        }

        // Check if user is courier
        var isCourier = validationResult.Value.Roles?.Any(r => r.Role?.Name == "Courier") ?? false;
        if (!isCourier)
        {
            return ApiResults.ToProblemDetails(Error.Forbidden("package.NOT_COURIER", "User is not a courier"));
        }

        _logger.LogInformation("Open locker {PostBoxId} for placement by courier", dto.PostBoxId);

        // In a real system, this would communicate with IoT locker
        // For now, just return success
        return Ok(new { message = "Locker opened", postBoxId = dto.PostBoxId });
    }

    [HttpPost("place")]
    public async Task<IActionResult> PlacePackageInLocker([FromBody] PlacePackageDto dto)
    {
        // Validate NFC card
        var validationResult = await _nfcService.ValidateNfcCardAsync(dto.SerialNumber);
        if (!validationResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(validationResult.Errors.First());
        }

        _logger.LogInformation("Place package {PackageId} in locker {PostBoxId}", dto.PackageId, dto.PostBoxId);

        var result = await _packageService.PlacePackageInLockerAsync(dto);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("locker/open-all-delivered")]
    public async Task<IActionResult> OpenAllDeliveredLockers([FromBody] OpenAllLockersDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
        {
            return ApiResults.ToProblemDetails(NfcErrors.NfcNotFound());
        }

        // Validate NFC card
        var validationResult = await _nfcService.ValidateNfcCardAsync(dto.SerialNumber);
        if (!validationResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(validationResult.Errors.First());
        }

        _logger.LogInformation("Opening all delivered lockers for user with NFC: {SerialNumber}", dto.SerialNumber);

        var result = await _packageService.OpenAllDeliveredLockersForUserAsync(dto.SerialNumber);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("{packageId}/receive")]
    public async Task<IActionResult> ReceivePackage(int packageId, [FromQuery] string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return ApiResults.ToProblemDetails(NfcErrors.NfcNotFound());
        }

        // Validate NFC card
        var validationResult = await _nfcService.ValidateNfcCardAsync(serialNumber);
        if (!validationResult.IsSuccess)
        {
            return ApiResults.ToProblemDetails(validationResult.Errors.First());
        }

        _logger.LogInformation("Receive package {PackageId} by user with NFC {SerialNumber}", packageId, serialNumber);

        var result = await _packageService.ReceivePackageAsync(packageId, serialNumber);

        return result.MatchNoData(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
}
