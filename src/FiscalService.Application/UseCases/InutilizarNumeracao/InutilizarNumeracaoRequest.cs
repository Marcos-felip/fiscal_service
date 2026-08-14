using MediatR;

namespace FiscalService.Application.UseCases.InutilizarNumeracao;

/// <summary>
/// Inutilizacao de faixa de numeracao.
///
/// **Nao e evento.** Ela age sobre uma faixa, nao sobre um documento: nao tem
/// chave de acesso nem protocolo de evento, e o servico da SEFAZ e outro. Por
/// isso o retorno tambem e proprio.
///
/// Serve para regularizar numero que foi reservado e nunca sera usado — falha
/// definitiva na emissao, salto na sequencia. Buraco na numeracao e apontamento.
/// </summary>
public class InutilizarNumeracaoRequest : IRequest<InutilizarNumeracaoResponse>
{
    /// <summary>CNPJ do emitente, so digitos.</summary>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Ano da numeracao inutilizada, com 4 digitos.</summary>
    public int Ano { get; set; }

    /// <summary>`55` para NF-e, `65` para NFC-e.</summary>
    public int Modelo { get; set; }

    public int Serie { get; set; }
    public int NumeroInicial { get; set; }
    public int NumeroFinal { get; set; }

    /// <summary>Motivo, de 15 a 255 caracteres.</summary>
    public string Justificativa { get; set; } = string.Empty;

    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;

    /// <summary>UF do emitente, usada para escolher o web service da SEFAZ.</summary>
    public string Uf { get; set; } = string.Empty;
}
