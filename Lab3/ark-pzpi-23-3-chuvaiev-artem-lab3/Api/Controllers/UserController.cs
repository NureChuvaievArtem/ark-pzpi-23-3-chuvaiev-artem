using Api.ApiResult;
using Api.Infrastructure.Errors;
using Api.Models.DTOs;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService, 
        IQrCodeService qrCodeService,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _qrCodeService = qrCodeService;
        _logger = logger;
    }
    
    [HttpPost("courier/register")]
    public async Task<IActionResult> RegisterCourier([FromBody] RegisterUserDto dto)
    {
        _logger.LogInformation("Courier registration request: {Email}", dto.EmailAddress);

        var result = await _userService.RegisterCourierAsync(dto);

        return result.Match(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }
    
    [HttpPost("client/register")]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterUserDto dto)
    {
        _logger.LogInformation("Client registration request: {Email}", dto.EmailAddress);

        var result = await _userService.RegisterClientAsync(dto);

        return result.Match(
            successStatusCode: 201,
            failure: ApiResults.ToProblemDetails
        );
    }
    
    [HttpGet("{userId}/nfc/qrcode/image")]
    [Produces("image/png")]
    public IActionResult GenerateQrCodeImageForNfcCollection(int userId)
    {
        _logger.LogInformation("QR code image generation request for user: {UserId}", userId);

        // Generate URL pointing to the NFC collection form
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var nfcCollectionUrl = $"{baseUrl}/api/nfc/form?userId={userId}";
        
        // Generate QR code image
        var qrCodeImageBytes = _qrCodeService.GenerateQrCodeImage(nfcCollectionUrl, pixelsPerModule: 10);
        
        return File(qrCodeImageBytes, "image/png");
    }
}
