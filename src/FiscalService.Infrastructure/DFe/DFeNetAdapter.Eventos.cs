using DFe.Classes.Flags;
using DFe.Utils;
using FiscalService.Application.Interfaces;
using FiscalService.Domain.Exceptions;
using FiscalService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NFe.Servicos;
using Ambiente = FiscalService.Domain.Enums.Ambiente;

namespace FiscalService.Infrastructure.DFe;

/// <summary>
/// Eventos fiscais alem do cancelamento: carta de correcao e inutilizacao.
///
/// Os dois ja existiam na DFe.NET — o adapter e que nao os usava. A configuracao
/// de versao (`VersaoRecepcaoEventoCceCancelamento`) esta de pe desde o MVP.
/// </summary>
public partial class DFeNetAdapter
{
    /// <summary>
    /// cStat 135 = evento registrado e vinculado a NF-e.
    /// cStat 136 = registrado, mas nao vinculado — acontece quando a nota ainda
    /// nao chegou a base da SEFAZ. A CC-e vale, e por isso tambem e sucesso.
    /// </summary>
    private static readonly int[] EventoRegistrado = { 135, 136 };

    public Task<EventoResultado> CartaCorrecao(
        string chaveAcesso,
        string correcao,
        int sequenciaEvento,
        string cpfCnpj,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            using var cert = LerCertificado(certificado);
            var configuracao = CriarConfiguracao(
                ambiente,
                UfDaChave(chaveAcesso),
                certificado,
                ModeloDaChave(chaveAcesso));

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.RecepcaoEventoCartaCorrecao(
                idlote: 1,
                sequenciaEvento: sequenciaEvento,
                chaveNFe: chaveAcesso,
                correcao: correcao,
                cpfcnpj: ApenasDigitos(cpfCnpj),
                dhEvento: null);

            var evento = retorno.Retorno?.retEvento?.FirstOrDefault()?.infEvento;
            var sucesso = evento is not null && EventoRegistrado.Contains(evento.cStat);

            var xml = retorno.ProcEventosNFe?.Count > 0
                ? FuncoesXml.ClasseParaXmlString(retorno.ProcEventosNFe[0])
                : retorno.RetornoStr;

            if (!sucesso)
            {
                _logger.LogWarning(
                    "Carta de correcao recusada: chave={Chave}, sequencia={Seq}, cStat={CStat} - {Motivo}",
                    chaveAcesso, sequenciaEvento, evento?.cStat, evento?.xMotivo);
            }

            return Task.FromResult(new EventoResultado(
                Sucesso: sucesso,
                Protocolo: evento?.nProt,
                XmlEvento: xml,
                MotivoRejeicao: sucesso
                    ? null
                    : evento?.xMotivo
                      ?? retorno.Retorno?.xMotivo
                      ?? "Resposta vazia da SEFAZ"));
        }
        catch (InvalidCertificateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na carta de correcao da chave {Chave}", chaveAcesso);
            return Task.FromResult(new EventoResultado(false, null, null, ex.Message));
        }
    }

    public Task<InutilizacaoResultado> Inutilizar(
        InutilizacaoPedido pedido,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            using var cert = LerCertificado(certificado);

            // A UF vem do pedido, e nao de uma chave: a inutilizacao age sobre
            // uma faixa que nunca virou documento, entao nao ha chave de onde
            // deduzi-la.
            var configuracao = CriarConfiguracao(
                ambiente,
                ParseEstado(pedido.Uf),
                certificado,
                ModeloDoNumero(pedido.Modelo));

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.NfeInutilizacao(
                cnpj: ApenasDigitos(pedido.Cnpj),
                ano: AnoDeDoisDigitos(pedido.Ano),
                modelo: ModeloDoNumero(pedido.Modelo),
                serie: pedido.Serie,
                numeroInicial: pedido.NumeroInicial,
                numeroFinal: pedido.NumeroFinal,
                justificativa: pedido.Justificativa);

            var info = retorno.Retorno?.infInut;

            // 102 = inutilizacao de numero homologada.
            var sucesso = info?.cStat == 102;

            if (!sucesso)
            {
                _logger.LogWarning(
                    "Inutilizacao recusada: serie={Serie}, faixa={Ini}-{Fim}, cStat={CStat} - {Motivo}",
                    pedido.Serie, pedido.NumeroInicial, pedido.NumeroFinal,
                    info?.cStat, info?.xMotivo);
            }

            return Task.FromResult(new InutilizacaoResultado(
                Sucesso: sucesso,
                Protocolo: info?.nProt,
                XmlInutilizacao: retorno.RetornoStr,
                MotivoRejeicao: sucesso
                    ? null
                    : info?.xMotivo ?? "Resposta vazia da SEFAZ"));
        }
        catch (InvalidCertificateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Erro ao inutilizar a faixa {Ini}-{Fim} da serie {Serie}",
                pedido.NumeroInicial, pedido.NumeroFinal, pedido.Serie);
            return Task.FromResult(new InutilizacaoResultado(false, null, null, ex.Message));
        }
    }

    /// <summary>
    /// Ano com dois digitos, como o layout da inutilizacao exige.
    ///
    /// O identificador do pedido e <c>ID + cUF + ano + CNPJ + modelo + serie +
    /// faixa</c>, com tamanho fixo. Mandar 2026 em vez de 26 estica o ID em dois
    /// caracteres e a SEFAZ devolve <c>215 - Falha no esquema XML</c>, sem dizer
    /// qual campo. Aconteceu na primeira inutilizacao real, em 14/08/2026.
    /// </summary>
    private static int AnoDeDoisDigitos(int ano) => ano % 100;

    /// <summary>Modelo pelo numero do layout — 55 ou 65.</summary>
    private static ModeloDocumento ModeloDoNumero(int modelo) => modelo switch
    {
        55 => ModeloDocumento.NFe,
        65 => ModeloDocumento.NFCe,
        _ => throw new ArgumentException($"Modelo de documento invalido: {modelo}"),
    };

    private static string ApenasDigitos(string? valor) =>
        new((valor ?? string.Empty).Where(char.IsDigit).ToArray());
}
