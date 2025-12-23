using Api.Infrastructure.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace Api.ApiResult;

public static class ResultExtensions
{
    public static IActionResult MatchNoData(
        this Result result,
        int successStatusCode,
        Func<Result, IActionResult>? failure = null
    )
    {
        if (result.IsSuccess)
        {
            return new StatusCodeResult(successStatusCode);
        }
        
        return failure != null
            ? failure(result)
            : ApiResults.ToProblemDetails(result);
    }
    
    public static IActionResult Match<T>(
        this Result<T> result,
        int successStatusCode,
        Func<Result<T>, IActionResult>? failure = null
    )
    {
        if (result.IsSuccess)
        {
            var body = new ApiResponse<T>
            {
                Data = result.Value
            };
            
            return new ObjectResult(body) { StatusCode = successStatusCode };
        }
        
        return failure != null
            ? failure(result)
            : ApiResults.ToProblemDetails(result);
    }
}