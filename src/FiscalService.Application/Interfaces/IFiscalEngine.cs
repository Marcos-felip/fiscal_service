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

    /// <summary>
    /// Transmite a Carta de Correcao (evento 110110).
    ///
    /// A sequencia chega pronta: se ela repete ou salta, e quantas correcoes a
    /// nota ja teve, sao perguntas sobre o historico do documento — e o motor
    /// nao persiste nada.
    /// </summary>
    Task<EventoResultado> CartaCorrecao(string chaveAcesso, string correcao, int sequenciaEvento, string cpfCnpj, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);

    /// <summary>
    /// Inutiliza uma faixa de numeracao.
    ///
    /// Nao e evento: age sobre uma faixa, nao sobre um documento, e por isso nao
    /// devolve chave de acesso.
    /// </summary>
    Task<InutilizacaoResultado> Inutilizar(InutilizacaoPedido pedido, Certificado certificado, Ambiente ambiente, CancellationToken ct = default);
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

/// <summary>Retorno de um evento vinculado a um documento — hoje, a CC-e.</summary>
public record EventoResultado(
    bool Sucesso,
    string? Protocolo,
    string? XmlEvento,
    string? MotivoRejeicao
);

/// <summary>
/// Pedido de inutilizacao. Agrupado num tipo proprio porque sao oito campos que
/// so fazem sentido juntos — e uma faixa, nao um documento.
/// </summary>
public record InutilizacaoPedido(
    string Cnpj,
    int Ano,
    int Modelo,
    int Serie,
    int NumeroInicial,
    int NumeroFinal,
    string Justificativa,
    string Uf
);

public record InutilizacaoResultado(
    bool Sucesso,
    string? Protocolo,
    string? XmlInutilizacao,
    string? MotivoRejeicao
);

public record StatusServicoResultado(
    bool Disponivel,
    string? Mensagem,
    int? TempoMedioResposta
);
