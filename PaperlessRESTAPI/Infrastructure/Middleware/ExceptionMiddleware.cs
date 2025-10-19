using PaperlessRESTAPI.Infrastructure.Exceptions;
using System.Net;
using System.Text.Json;

namespace PaperlessRESTAPI.Infrastructure.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing request {RequestPath}", 
                context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        object response;

        switch (exception)
        {
            case BusinessException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    error = new
                    {
                        code = businessEx.ErrorCode,
                        message = businessEx.Message,
                        type = "BusinessError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;

            case DataException:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response = new
                {
                    error = new
                    {
                        message = "Data processing error occurred",
                        type = "DataError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;

            case ServiceException:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response = new
                {
                    error = new
                    {
                        message = "External service error occurred",
                        type = "ServiceError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;

            case FileNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    error = new
                    {
                        message = "File not found",
                        type = "NotFoundError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    error = new
                    {
                        message = "Unauthorized access",
                        type = "UnauthorizedError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    error = new
                    {
                        message = exception.Message,
                        type = "ArgumentError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    error = new
                    {
                        message = "An internal server error occurred",
                        type = "InternalError",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
