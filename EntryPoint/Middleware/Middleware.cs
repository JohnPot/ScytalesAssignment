using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace EntryPoint.Middleware;

public class Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<Middleware> _logger;

    public Middleware(RequestDelegate next, ILogger<Middleware> logger)
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
        catch(DbUpdateException ex)
        {
            _logger.LogError(ex.ToString(), "Unhandled exception occurred with response code {responseCode}", context.Response.StatusCode);

            await HandleExceptionAsync(context, ex, ExceptionStates.DuplicateException);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString(), "Unhandled exception occurred with response code {responseCode}", context.Response.StatusCode);

            await HandleExceptionAsync(context, ex, ExceptionStates.NormalException);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex, ExceptionStates exceptionStates)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            error = "An unexpected error occurred",
            detail = exceptionStates == ExceptionStates.DuplicateException ? "Cannot enter unique value again, please change your input" :  ex.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    public enum ExceptionStates
    {
        NormalException,
        DuplicateException
    }
}