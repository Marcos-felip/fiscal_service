namespace FiscalService.Application.UseCases.StatusServico;

public class StatusServicoResponse
{
    public bool Disponivel { get; set; }
    public string? Mensagem { get; set; }
    public int? TempoMedioResposta { get; set; }
}
