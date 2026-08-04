using FiscalService.Application.UseCases.EmitirNfce;
using FiscalService.Application.UseCases.ConsultarNfce;
using FiscalService.Application.UseCases.CancelarNfce;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FiscalService.Api.Controllers;

[ApiController]
[Route("api/nfce")]
[Produces("application/json")]
public class NfceController : ControllerBase
{
    private readonly IMediator _mediator;

    public NfceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("emit")]
    [ProducesResponseType(typeof(EmitirNfceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Emitir([FromBody] EmitirNfceRequest request)
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
}
