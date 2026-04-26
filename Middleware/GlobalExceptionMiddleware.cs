namespace ms_filtering_ia.Middleware;

/// <summary>
/// Intercepte toutes les exceptions non gérées et retourne une réponse JSON propre.
/// Évite les stack traces en production et garantit les codes HTTP corrects.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ressource introuvable.");
            await WriteJsonAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Opération invalide.");
            await WriteJsonAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée.");
            await WriteJsonAsync(context, StatusCodes.Status500InternalServerError,
                "Une erreur interne est survenue.");
        }
    }

    private static Task WriteJsonAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            timestamp = DateTime.UtcNow,
            message
        });
    }
}