using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FiscalService.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private const string HEALTH_PATH = "/health";
    private const string SWAGGER_PATH = "/swagger";

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (path == HEALTH_PATH || (path != null && path.StartsWith(SWAGGER_PATH)))
        {
            await _next(context);
            return;
        }

        var apiKeyHeaderName = _configuration["ApiKey:Name"] ?? "X-Api-Key";
        var expectedApiKey = _configuration["ApiKey:Value"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(apiKeyHeaderName, out var providedApiKey) ||
            providedApiKey != expectedApiKey)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"codigo\":\"AUTH_FALHA\",\"mensagem\":\"API Key invalida ou ausente\"}");
            return;
        }

        await _next(context);
    }
}
