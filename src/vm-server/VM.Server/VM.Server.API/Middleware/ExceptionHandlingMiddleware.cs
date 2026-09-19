using VM.Server.API.DTO;
using VM.Server.Domain.Errors;

namespace VM.Server.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            var statusCode = MapStatusCode(ex.Code);
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "DomainException with unmapped code {Code}", ex.Code);
            }
            else
            {
                logger.LogInformation("Business refusal {Code}: {Message}", ex.Code, ex.Message);
            }

            await WriteErrorAsync(context, statusCode, new ErrorResponseDTO(ex.Code, ex.Message, ex.Details));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                new ErrorResponseDTO("INTERNAL_ERROR", "An unexpected error occurred.", null));
        }
    }

    private static int MapStatusCode(string code) => code switch
    {
        ErrorCodes.InvalidDenomination => StatusCodes.Status400BadRequest,
        ErrorCodes.InsufficientFunds => StatusCodes.Status400BadRequest,
        ErrorCodes.OutOfStock => StatusCodes.Status400BadRequest,
        ErrorCodes.InvalidQuantity => StatusCodes.Status400BadRequest,
        ErrorCodes.InvalidPrice => StatusCodes.Status400BadRequest,
        ErrorCodes.InvalidProduct => StatusCodes.Status400BadRequest,
        ErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
        ErrorCodes.DuplicateProduct => StatusCodes.Status409Conflict,
        ErrorCodes.DuplicatePrice => StatusCodes.Status409Conflict,
        ErrorCodes.ChangeUnavailable => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static Task WriteErrorAsync(HttpContext context, int statusCode, ErrorResponseDTO body)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(body);
    }
}
