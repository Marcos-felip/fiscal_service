using FiscalService.Application.UseCases.CartaCorrecao;
using FiscalService.Application.UseCases.InutilizarNumeracao;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FiscalService.Api.Controllers;

/// <summary>
/// Eventos fiscais que nao sao emissao nem cancelamento.
///
/// O cancelamento continua sob `/api/nfce/cancel` e `/api/nfe/cancel`, onde
/// sempre esteve — move-lo para ca quebraria o backend sem ganho nenhum.
/// </summary>
[ApiController]
[Route("api/eventos")]
[Produces("application/json")]
public class EventosController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("carta-correcao")]
    [ProducesResponseType(typeof(CartaCorrecaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CartaCorrecao([FromBody] CartaCorrecaoRequest request)
    {
        var response = await _mediator.Send(request);
        return response.Sucesso ? Ok(response) : BadRequest(response);
    }

    [HttpPost("inutilizar")]
    [ProducesResponseType(typeof(InutilizarNumeracaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inutilizar(
        [FromBody] InutilizarNumeracaoRequest request)
    {
        var response = await _mediator.Send(request);
        return response.Sucesso ? Ok(response) : BadRequest(response);
    }
}
