using FiscalService.Application.Interfaces;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Infrastructure.DFe;

public class DFeNetAdapter : IFiscalEngine
{
    public Task<EmitirNfceResultado> Emitir(Nfce nfce, Certificado certificado, Csc csc, Ambiente ambiente, CancellationToken ct = default)
    {
        throw new NotImplementedException("Implementar integracao com DFe.NET para emissao de NFC-e");
    }

    public Task<ConsultarNfceResultado> Consultar(string chaveAcesso, Certificado certificado, Ambiente ambiente, CancellationToken ct = default)
    {
        throw new NotImplementedException("Implementar integracao com DFe.NET para consulta de NFC-e");
    }

    public Task<CancelarNfceResultado> Cancelar(string chaveAcesso, string protocolo, string justificativa, Certificado certificado, Ambiente ambiente, CancellationToken ct = default)
    {
        throw new NotImplementedException("Implementar integracao com DFe.NET para cancelamento de NFC-e");
    }

    public Task<StatusServicoResultado> StatusServico(Ambiente ambiente, string uf, CancellationToken ct = default)
    {
        throw new NotImplementedException("Implementar integracao com DFe.NET para status do servico SEFAZ");
    }
}
