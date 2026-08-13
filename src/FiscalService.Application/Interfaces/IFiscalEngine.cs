using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Application.Interfaces;

public interface IFiscalEngine
{
    Task<EmitirNfceResultado> Emitir(Nfce nfce, Certificado certificado, Csc csc, Ambiente ambiente, CancellationToken ct = default);

    /// <summary>
    /// Emite NF-e modelo 55. Nao recebe CSC: o codigo e exclusivo da NFC-e, e o
    /// <c>QrCode</c> do resultado vem sempre nulo aqui — a NF-e nao tem QR de consulta.
    /// </summary>
    Task<EmitirNfceResultado> EmitirNfe(Nfe nfe, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);
    Task<ConsultarNfceResultado> Consultar(string chaveAcesso, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);
    Task<CancelarNfceResultado> Cancelar(string chaveAcesso, string protocolo, string justificativa, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);

    /// <summary>
    /// Os web services da SEFAZ exigem certificado do contribuinte no handshake TLS,
    /// inclusive para consulta de status. Por isso o certificado tambem e obrigatorio aqui.
    /// </summary>
    Task<StatusServicoResultado> StatusServico(Ambiente ambiente, string uf, Certificado certificado, CancellationToken ct = default);
}

public record EmitirNfceResultado(
    bool Sucesso,
    string? ChaveAcesso,
    string? Protocolo,
    string? XmlAutorizado,
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
