namespace ms_filtering_ia.Middleware;

public sealed class GatewayAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _internalSecretKey;

    public GatewayAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _internalSecretKey = configuration["Gateway:InternalSecretKey"]
            ?? throw new InvalidOperationException("Gateway:InternalSecretKey is not configured.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var internalKey = context.Request.Headers["X-Internal-Secret-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(internalKey) || internalKey != _internalSecretKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                timestamp = DateTime.UtcNow,
                message   = "Unauthorized: request did not pass through the gateway."
            });
            return;
        }

        var role = context.Request.Headers["X-User-Role"].FirstOrDefault();

        if (role != "HR")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                timestamp = DateTime.UtcNow,
                message   = "Forbidden: insufficient role."
            });
            return;
        }

        var cin = context.Request.Headers["X-User-CIN"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cin))
            context.Items["UserCin"] = cin;

        await _next(context);
    }
}