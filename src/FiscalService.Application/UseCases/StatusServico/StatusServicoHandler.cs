using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using MediatR;

namespace FiscalService.Application.UseCases.StatusServico;

public class StatusServicoHandler : IRequestHandler<StatusServicoRequest, StatusServicoResponse>
{
    private readonly IFiscalEngine _fiscalEngine;

    public StatusServicoHandler(IFiscalEngine fiscalEngine)
    {
        _fiscalEngine = fiscalEngine;
    }

    public async Task<StatusServicoResponse> Handle(StatusServicoRequest request, CancellationToken cancellationToken)
    {
        var ambiente = NfceMapper.ToAmbiente(request.Ambiente);

        var resultado = await _fiscalEngine.StatusServico(ambiente, request.Uf, cancellationToken);

        return new StatusServicoResponse
        {
            Disponivel = resultado.Disponivel,
            Mensagem = resultado.Mensagem,
            TempoMedioResposta = resultado.TempoMedioResposta
        };
    }
}
