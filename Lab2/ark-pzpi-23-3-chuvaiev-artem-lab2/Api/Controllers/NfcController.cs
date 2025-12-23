using Api.ApiResult;
using Api.Infrastructure.Errors;
using Api.Models;
using Api.Models.DTOs;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NfcController : ControllerBase
{
    private readonly INfcService _nfcService;
    private readonly ILogger<NfcController> _logger;

    public NfcController(
        INfcService nfcService,
        ILogger<NfcController> logger)
    {
        _nfcService = nfcService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostNfcData([FromBody] RegisterNfcDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SerialNumber) || dto.UserId <= 0)
        {
            return ApiResults.ToProblemDetails(Error.Validation("nfc.INVALID_DATA", "Serial number and UserId are required"));
        }

        _logger.LogInformation("Receiving NFC data from client: SerialNumber={SerialNumber}, UserId={UserId}", 
            dto.SerialNumber, dto.UserId);

        var result = await _nfcService.RegisterNfcToUserAsync(dto.UserId, dto.SerialNumber);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsersWithNfc()
    {
        var result = await _nfcService.GetAllUsersWithNfcAsync();
        
        if (!result.IsSuccess)
        {
            return ApiResults.ToProblemDetails(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("serial/{serialNumber}/user")]
    public async Task<IActionResult> GetUserIdBySerialNumber(string serialNumber)
    {
        var result = await _nfcService.GetUserIdByNfcSerialAsync(serialNumber);

        return result.Match(
            successStatusCode: 200, 
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpGet("user/{userId}/serial")]
    public async Task<IActionResult> GetNfcSerialByUserId(int userId)
    {
        var result = await _nfcService.GetNfcSerialByUserIdAsync(userId);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateNfcCard([FromBody] ValidateNfcDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
        {
            return ApiResults.ToProblemDetails(NfcErrors.NfcNotFound());
        }

        _logger.LogInformation("Validating NFC card: {SerialNumber}", dto.SerialNumber);

        var result = await _nfcService.ValidateNfcCardAsync(dto.SerialNumber);

        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }
    
    [HttpGet("form")]
    [Produces("text/html")]
    public IActionResult GetNfcCollectionForm([FromQuery] int? userId = null)
    {
        var userIdValue = userId?.ToString() ?? "";
        var html = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>NFC Data Collection</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        .container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            padding: 40px;
            max-width: 500px;
            width: 100%;
        }
        h1 {
            color: #333;
            margin-bottom: 10px;
            font-size: 28px;
            text-align: center;
        }
        .subtitle {
            color: #666;
            text-align: center;
            margin-bottom: 30px;
            font-size: 14px;
        }
        .form-group {
            margin-bottom: 25px;
        }
        label {
            display: block;
            margin-bottom: 8px;
            color: #333;
            font-weight: 600;
            font-size: 14px;
        }
        input {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            font-size: 16px;
            transition: border-color 0.3s;
        }
        input:focus {
            outline: none;
            border-color: #667eea;
        }
        button {
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        button:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.4);
        }
        button:active {
            transform: translateY(0);
        }
        .message {
            margin-top: 20px;
            padding: 12px;
            border-radius: 10px;
            text-align: center;
            font-size: 14px;
            display: none;
        }
        .message.success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        .message.error {
            background: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        .message.show {
            display: block;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <h1>📱 NFC Data Collection</h1>
        <p class=""subtitle"">Please enter your NFC card information</p>
        <form id=""nfcForm"">
            <div class=""form-group"">
                <label for=""userId"">User ID:</label>
                <input type=""number"" id=""userId"" name=""userId"" required placeholder=""Enter your user ID"" value=""" + userIdValue + @""">
            </div>
            <div class=""form-group"">
                <label for=""serialNumber"">NFC Serial Number:</label>
                <input type=""text"" id=""serialNumber"" name=""serialNumber"" required placeholder=""Enter NFC serial number"">
            </div>
            <button type=""submit"">Submit NFC Data</button>
        </form>
        <div id=""message"" class=""message""></div>
    </div>
    <script>
        document.getElementById('nfcForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const userId = document.getElementById('userId').value;
            const serialNumber = document.getElementById('serialNumber').value;
            const messageDiv = document.getElementById('message');
            
            try {
                const response = await fetch('/api/nfc', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        userId: parseInt(userId),
                        serialNumber: serialNumber
                    })
                });
                
                const data = await response.json();
                
                if (response.ok) {
                    messageDiv.className = 'message success show';
                    messageDiv.textContent = 'NFC data successfully submitted!';
                    document.getElementById('nfcForm').reset();
                } else {
                    messageDiv.className = 'message error show';
                    messageDiv.textContent = data.detail || 'An error occurred. Please try again.';
                }
            } catch (error) {
                messageDiv.className = 'message error show';
                messageDiv.textContent = 'Network error. Please check your connection and try again.';
            }
        });
    </script>
</body>
</html>";
        return Content(html, "text/html");
    }
}