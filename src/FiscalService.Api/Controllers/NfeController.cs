using FiscalService.Application.Interfaces;
using FiscalService.Application.UseCases.CancelarNfce;
using FiscalService.Application.UseCases.ConsultarNfce;
using FiscalService.Application.UseCases.EmitirNfe;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FiscalService.Api.Controllers;

/// <summary>
/// NF-e modelo 55.
///
/// Cancelamento e consulta reaproveitam os casos de uso da NFC-e: os eventos da SEFAZ nao
/// distinguem modelo, e a UF e o CNPJ saem da propria chave de acesso. As rotas existem aqui
/// por clareza de contrato, para que o chamador nao precise chamar <c>/api/nfce/*</c> para
/// tratar de uma NF-e.
/// </summary>
[ApiController]
[Route("api/nfe")]
[Produces("application/json")]
public class NfeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDanfeNfeGenerator _danfeGenerator;

    public NfeController(IMediator mediator, IDanfeNfeGenerator danfeGenerator)
    {
        _mediator = mediator;
        _danfeGenerator = danfeGenerator;
    }

    [HttpPost("emit")]
    [ProducesResponseType(typeof(EmitirNfeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Emitir([FromBody] EmitirNfeRequest request)
    {
        var response = await _mediator.Send(request);
        return response.Sucesso ? Ok(response) : BadRequest(response);
    }

    [HttpPost("consulta")]
    [ProducesResponseType(typeof(ConsultarNfceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Consultar([FromBody] ConsultarNfceRequest request)
    {
        var response = await _mediator.Send(request);
        return response.Sucesso ? Ok(response) : BadRequest(response);
    }

    [HttpPost("cancel")]
    [ProducesResponseType(typeof(CancelarNfceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar([FromBody] CancelarNfceRequest request)
    {
        var response = await _mediator.Send(request);
        return response.Sucesso ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Regera o DANFE a partir do XML autorizado. Util quando o chamador perdeu o arquivo ou
    /// precisa reimprimir sem guardar o binario.
    /// </summary>
    [HttpPost("danfe")]
    [ProducesResponseType(typeof(GerarDanfeNfeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Danfe([FromBody] GerarDanfeNfeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.XmlAutorizado))
            return BadRequest(new { mensagem = "XML autorizado e obrigatorio" });

        var danfe = _danfeGenerator.Gerar(request.XmlAutorizado);

        return Ok(new GerarDanfeNfeResponse(
            Convert.ToBase64String(danfe.Conteudo),
            danfe.ContentType));
    }
}

public record GerarDanfeNfeRequest(string XmlAutorizado);

public record GerarDanfeNfeResponse(string DanfeBase64, string DanfeContentType);
