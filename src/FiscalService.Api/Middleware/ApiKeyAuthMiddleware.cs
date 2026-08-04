using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FiscalService.Api.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private const string HEALTH_PATH = "/health";
    private const string SWAGGER_PATH = "/swagger";
    private const string CHAVE_CONFIGURACAO = "ApiKey:Value";

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;

        // O pipeline e montado no start da aplicacao, entao isto derruba o boot em vez
        // de deixar o servico subir sem autenticacao.
        if (string.IsNullOrWhiteSpace(configuration[CHAVE_CONFIGURACAO]))
        {
            throw new InvalidOperationException(
                $"'{CHAVE_CONFIGURACAO}' nao configurada. Defina a variavel de ambiente " +
                "FISCAL_API_KEY (ou ApiKey__Value) antes de subir o servico.");
        }
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
        var expectedApiKey = _configuration[CHAVE_CONFIGURACAO];

        // Rede de seguranca: a configuracao pode ser recarregada em runtime e ficar vazia.
        // Nesse caso a requisicao e recusada, nunca liberada.
        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            _logger.LogError(
                "'{Chave}' esta vazia em runtime; requisicoes serao recusadas", CHAVE_CONFIGURACAO);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"codigo\":\"CONFIG_INVALIDA\",\"mensagem\":\"Servico mal configurado\"}");
            return;
        }

        if (!context.Request.Headers.TryGetValue(apiKeyHeaderName, out var providedApiKey) ||
            !ChaveConfere(providedApiKey.ToString(), expectedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"codigo\":\"AUTH_FALHA\",\"mensagem\":\"API Key invalida ou ausente\"}");
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Compara em tempo constante, para nao vazar a chave por timing.
    /// As chaves sao reduzidas a SHA-256 antes da comparacao porque
    /// <see cref="CryptographicOperations.FixedTimeEquals"/> retorna de imediato quando os
    /// tamanhos diferem — o hash deixa os dois lados sempre com 32 bytes.
    /// </summary>
    private static bool ChaveConfere(string fornecida, string esperada)
    {
        Span<byte> hashFornecida = stackalloc byte[32];
        Span<byte> hashEsperada = stackalloc byte[32];

        SHA256.HashData(Encoding.UTF8.GetBytes(fornecida), hashFornecida);
        SHA256.HashData(Encoding.UTF8.GetBytes(esperada), hashEsperada);

        return CryptographicOperations.FixedTimeEquals(hashFornecida, hashEsperada);
    }
}
