using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Application.Interfaces;

public interface IFiscalEngine
{
    Task<EmitirNfceResultado> Emitir(Nfce nfce, Certificado certificado, Csc csc, Ambiente ambiente, CancellationToken ct = default);
    Task<ConsultarNfceResultado> Consultar(string chaveAcesso, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);
    Task<CancelarNfceResultado> Cancelar(string chaveAcesso, string protocolo, string justificativa, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);
    Task<StatusServicoResultado> StatusServico(Ambiente ambiente, string uf, CancellationToken ct = default);
}

public record EmitirNfceResultado(
    bool Sucesso,
    string? ChaveAcesso,
    string? Protocolo,
    string? XmlAutorizado,
    byte[]? DanfePdf,
    string? QrCode,
    string? MotivoRejeicao,
    string? CodigoRejeicao
);

public record ConsultarNfceResultado(
    bool Sucesso,
    string Status,
    string? Protocolo,
    string? XmlConsulta
);

public record CancelarNfceResultado(
    bool Sucesso,
    string? Protocolo,
    string? XmlCancelamento,
    string? MotivoRejeicao
);

public record StatusServicoResultado(
    bool Disponivel,
    string? Mensagem,
    int? TempoMedioResposta
);
