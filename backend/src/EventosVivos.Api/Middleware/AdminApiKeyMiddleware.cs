using EventosVivos.Api.Serialization;

namespace EventosVivos.Api.Middleware;

public sealed class AdminApiKeyMiddleware
{
    private const string HeaderName = "X-Admin-Key";
    private readonly RequestDelegate _next;

    public AdminApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (RequiresAdminKey(context.Request))
        {
            var expectedKey = configuration["Admin:ApiKey"];

            if (!string.IsNullOrWhiteSpace(expectedKey))
            {
                if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey)
                    || providedKey != expectedKey)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(
                        new ErrorResponse("UNAUTHORIZED", "Se requiere clave de administrador."),
                        ApiJsonOptions.Default);
                    return;
                }
            }
        }

        await _next(context);
    }

    private static bool RequiresAdminKey(HttpRequest request) =>
        HttpMethods.IsPost(request.Method)
        && request.Path.Value?.EndsWith("/confirm-payment", StringComparison.OrdinalIgnoreCase) == true;
}
