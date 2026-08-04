using FiscalService.Application.UseCases.StatusServico;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FiscalService.Api.Controllers;

[ApiController]
[Route("api/sefaz")]
[Produces("application/json")]
public class SefazController : ControllerBase
{
    private readonly IMediator _mediator;

    public SefazController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("status-servico")]
    [ProducesResponseType(typeof(StatusServicoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StatusServico([FromBody] StatusServicoRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
