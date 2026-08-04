using MediatR;

namespace FiscalService.Application.UseCases.StatusServico;

public class StatusServicoRequest : IRequest<StatusServicoResponse>
{
    public string Ambiente { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
}
