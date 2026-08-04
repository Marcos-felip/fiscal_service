using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FiscalService.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
        catch (Domain.Exceptions.FiscalRejectionException ex)
        {
            _logger.LogWarning(ex, "Rejeicao fiscal");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteErrorResponse(context, ex.CodigoRejeicao, ex.Motivo);
        }
        catch (Domain.Exceptions.InvalidCertificateException ex)
        {
            _logger.LogWarning(ex, "Certificado invalido");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteErrorResponse(context, "CERT_INVALIDO", ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento invalido");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteErrorResponse(context, "ARG_INVALIDO", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteErrorResponse(context, "ERRO_INTERNO", "Erro interno do servidor");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, string codigo, string mensagem)
    {
        context.Response.ContentType = "application/json";
        var response = new { codigo, mensagem, timestamp = DateTime.UtcNow };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
