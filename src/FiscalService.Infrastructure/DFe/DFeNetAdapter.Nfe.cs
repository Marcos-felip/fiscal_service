using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using DFe.Utils;
using FiscalService.Application.Interfaces;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Exceptions;
using FiscalService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NFe.Classes;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Cobranca;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Transporte;
using NFe.Classes.Servicos.Tipos;
using NFe.Servicos;
using NFe.Utils;
using NFe.Utils.NFe;
using Ambiente = FiscalService.Domain.Enums.Ambiente;
using DomainFinalidade = FiscalService.Domain.Enums.FinalidadeNfe;
using DomainIndicadorIe = FiscalService.Domain.Enums.IndicadorIeDestinatario;
using DomainModalidadeFrete = FiscalService.Domain.Enums.ModalidadeFrete;
using DomainPresenca = FiscalService.Domain.Enums.PresencaComprador;
using DomainTipoOperacao = FiscalService.Domain.Enums.TipoOperacao;
using NFeClasse = NFe.Classes.NFe;

namespace FiscalService.Infrastructure.DFe;

/// <summary>
/// Metade NF-e do adapter. Fica em arquivo proprio porque os enums de dominio
/// (<c>FinalidadeNfe</c>, <c>PresencaComprador</c>, <c>ModalidadeFrete</c>) colidem de nome
/// com os da DFe.NET — separar deixa os alias contidos aqui em vez de espalhados.
///
/// O que e comum aos dois modelos — certificado, configuracao, emitente, detalhe do item,
/// totais — vem da outra metade, sem copia.
/// </summary>
public partial class DFeNetAdapter
{
    private const string NomeDestinatarioHomologacaoNfe =
        "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

    public Task<EmitirNfceResultado> EmitirNfe(
        Nfe nfe,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            var uf = ParseEstado(nfe.Emitente.Endereco.Uf);
            using var cert = LerCertificado(certificado);
            var configuracao = CriarConfiguracao(ambiente, uf, certificado, ModeloDocumento.NFe);

            // Sem infNFeSupl: QR Code e URL de consulta sao da NFC-e.
            var documento = MontarNfe55(nfe, ambiente, uf).Assina(configuracao, cert);

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.NFeAutorizacao(
                idLote: 1,
                indSinc: IndicadorSincronizacao.Sincrono,
                nFes: new List<NFeClasse> { documento },
                compactarMensagem: false);

            var protocolo = retorno.Retorno?.protNFe?.infProt;

            if (protocolo is null)
            {
                return Task.FromResult(new EmitirNfceResultado(
                    Sucesso: false,
                    ChaveAcesso: null,
                    Protocolo: null,
                    XmlAutorizado: null,
                    QrCode: null,
                    MotivoRejeicao: retorno.Retorno?.xMotivo ?? "Resposta da SEFAZ sem protocolo",
                    CodigoRejeicao: retorno.Retorno?.cStat.ToString() ?? "000"));
            }

            if (protocolo.cStat != 100)
            {
                _logger.LogWarning("NF-e rejeitada pela SEFAZ: {CStat} - {Motivo}",
                    protocolo.cStat, protocolo.xMotivo);

                return Task.FromResult(new EmitirNfceResultado(
                    Sucesso: false,
                    ChaveAcesso: protocolo.chNFe,
                    Protocolo: null,
                    XmlAutorizado: null,
                    QrCode: null,
                    MotivoRejeicao: protocolo.xMotivo,
                    CodigoRejeicao: protocolo.cStat.ToString()));
            }

            var proc = new nfeProc
            {
                versao = VersaoLayoutNfe,
                NFe = documento,
                protNFe = retorno.Retorno!.protNFe
            };

            return Task.FromResult(new EmitirNfceResultado(
                Sucesso: true,
                ChaveAcesso: protocolo.chNFe,
                Protocolo: protocolo.nProt,
                XmlAutorizado: FuncoesXml.ClasseParaXmlString(proc),
                QrCode: null,
                MotivoRejeicao: null,
                CodigoRejeicao: null));
        }
        catch (InvalidCertificateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir NF-e serie {Serie} numero {Numero}", nfe.Serie, nfe.Numero);
            return Task.FromResult(new EmitirNfceResultado(
                Sucesso: false,
                ChaveAcesso: null,
                Protocolo: null,
                XmlAutorizado: null,
                QrCode: null,
                MotivoRejeicao: ex.Message,
                CodigoRejeicao: "ERRO"));
        }
    }

    // ---------------------------------------------------------------- montagem do modelo 55

    internal static NFeClasse MontarNfe55(Nfe nfe, Ambiente ambiente, Estado uf)
    {
        if (nfe.Itens.Count == 0)
            throw new ArgumentException("NF-e sem itens");

        if (nfe.Pagamentos.Count == 0)
            throw new ArgumentException("NF-e sem formas de pagamento");

        var emitente = nfe.Emitente;
        var codigoNumerico = GerarCodigoNumerico();
        var tpAmb = ambiente == Ambiente.Producao ? TipoAmbiente.Producao : TipoAmbiente.Homologacao;

        var chave = ChaveFiscal.ObterChave(
            ufEmitente: uf,
            dataEmissao: nfe.DataEmissao,
            cnpjEmitente: emitente.Cnpj.Valor,
            modelo: ModeloDocumento.NFe,
            serie: nfe.Serie,
            numero: nfe.Numero,
            tipoEmissao: (int)TipoEmissao.teNormal,
            cNf: int.Parse(codigoNumerico));

        var documento = new NFeClasse
        {
            infNFe = new infNFe
            {
                versao = VersaoLayoutNfe,
                Id = $"NFe{chave.Chave}",
                ide = new ide
                {
                    cUF = uf,
                    cNF = codigoNumerico,
                    natOp = nfe.NaturezaOperacao,
                    mod = ModeloDocumento.NFe,
                    serie = nfe.Serie,
                    nNF = nfe.Numero,
                    dhEmi = nfe.DataEmissao,
                    tpNF = MapearTipoOperacao(nfe.TipoOperacao),
                    // O recorte e operacao interna, garantido pelo validador antes de chegar aqui.
                    idDest = DestinoOperacao.doInterna,
                    cMunFG = long.Parse(emitente.Endereco.CodigoMunicipio),
                    tpImp = TipoImpressao.tiRetrato,
                    tpEmis = TipoEmissao.teNormal,
                    cDV = chave.DigitoVerificador,
                    tpAmb = tpAmb,
                    finNFe = MapearFinalidade(nfe.Finalidade),
                    indFinal = nfe.ConsumidorFinal
                        ? ConsumidorFinal.cfConsumidorFinal
                        : ConsumidorFinal.cfNao,
                    indPres = MapearPresenca(nfe.Presenca),
                    procEmi = ProcessoEmissao.peAplicativoContribuinte,
                    verProc = VersaoAplicativo
                },
                emit = MontarEmitente(emitente, uf),
                dest = MontarDestinatarioNfe(nfe, ambiente),
                det = nfe.Itens
                    .Select((item, indice) => MontarDetalhe(item, ambiente, indice == 0))
                    .ToList(),
                total = MontarTotal(nfe.Itens),
                transp = MontarTransporte(nfe.Transporte),
                cobr = MontarCobranca(nfe.Cobranca),
                pag = new List<pag>
                {
                    new()
                    {
                        // Mesma montagem da NFC-e: o grupo de cartoes exigido no
                        // pagamento eletronico vale para os dois modelos.
                        detPag = nfe.Pagamentos.Select(MontarDetalhePagamento).ToList()
                    }
                }
            }
        };

        return documento;
    }

    /// <summary>
    /// Destinatario da NF-e: sempre presente, sempre com endereco. Em homologacao o nome e
    /// obrigatoriamente substituido pelo texto da NT 2015/002 — o endereco e o documento
    /// continuam os reais.
    /// </summary>
    private static dest MontarDestinatarioNfe(Nfe nfe, Ambiente ambiente)
    {
        var destinatario = nfe.Destinatario;
        var endereco = destinatario.Endereco;

        var grupo = new dest(VersaoServicoNfe)
        {
            xNome = ambiente == Ambiente.Homologacao
                ? NomeDestinatarioHomologacaoNfe
                : destinatario.Nome,
            indIEDest = MapearIndicadorIe(destinatario.IndicadorIe),
            IE = destinatario.InscricaoEstadual,
            email = destinatario.Email,
            enderDest = new enderDest
            {
                xLgr = endereco.Logradouro,
                nro = endereco.Numero,
                xCpl = endereco.Complemento,
                xBairro = endereco.Bairro,
                cMun = long.Parse(endereco.CodigoMunicipio),
                xMun = endereco.Municipio,
                // enderDest.UF e string, ao contrario de enderEmit.UF, que e Estado.
                UF = ParseEstado(endereco.Uf).ToString(),
                CEP = endereco.Cep,
                cPais = 1058,
                xPais = "BRASIL",
                fone = ParseTelefone(destinatario.Telefone)
            }
        };

        if (destinatario.Documento.EhCnpj)
            grupo.CNPJ = destinatario.Documento.Valor;
        else
            grupo.CPF = destinatario.Documento.Valor;

        return grupo;
    }

    private static transp MontarTransporte(Domain.ValueObjects.Transporte transporte)
    {
        var grupo = new transp { modFrete = MapearModalidadeFrete(transporte.Modalidade) };

        if (transporte.Transportadora is not null)
        {
            var t = transporte.Transportadora;
            grupo.transporta = new transporta
            {
                xNome = t.Nome,
                IE = t.InscricaoEstadual,
                xEnder = t.EnderecoCompleto,
                xMun = t.Municipio,
                UF = string.IsNullOrWhiteSpace(t.Uf) ? null : ParseEstado(t.Uf).ToString()
            };

            if (t.Documento.EhCnpj)
                grupo.transporta.CNPJ = t.Documento.Valor;
            else
                grupo.transporta.CPF = t.Documento.Valor;
        }

        if (transporte.Veiculo is not null)
        {
            grupo.veicTransp = new veicTransp
            {
                placa = transporte.Veiculo.Placa,
                UF = ParseEstado(transporte.Veiculo.Uf).ToString(),
                RNTC = transporte.Veiculo.Rntc
            };
        }

        if (transporte.Volumes.Count > 0)
        {
            grupo.vol = transporte.Volumes
                .Select(v => new vol
                {
                    qVol = v.Quantidade,
                    esp = v.Especie,
                    marca = v.Marca,
                    nVol = v.Numeracao,
                    pesoL = v.PesoLiquido,
                    pesoB = v.PesoBruto
                })
                .ToList();
        }

        return grupo;
    }

    private static cobr? MontarCobranca(Domain.ValueObjects.Cobranca? cobranca)
    {
        if (cobranca is null)
            return null;

        var grupo = new cobr();

        if (cobranca.NumeroFatura is not null || cobranca.ValorOriginal.HasValue)
        {
            grupo.fat = new fat
            {
                nFat = cobranca.NumeroFatura,
                vOrig = cobranca.ValorOriginal,
                vDesc = cobranca.ValorDesconto,
                vLiq = cobranca.ValorLiquido
            };
        }

        if (cobranca.Duplicatas.Count > 0)
        {
            grupo.dup = cobranca.Duplicatas
                .Select(d => new dup
                {
                    nDup = d.Numero,
                    dVenc = d.Vencimento,
                    vDup = d.Valor
                })
                .ToList();
        }

        return grupo;
    }

    // ---------------------------------------------------------------- mapeamentos de enum

    private static TipoNFe MapearTipoOperacao(DomainTipoOperacao tipo) => tipo switch
    {
        DomainTipoOperacao.Entrada => TipoNFe.tnEntrada,
        DomainTipoOperacao.Saida => TipoNFe.tnSaida,
        _ => throw new ArgumentException($"Tipo de operacao invalido: {tipo}")
    };

    private static FinalidadeNFe MapearFinalidade(DomainFinalidade finalidade) => finalidade switch
    {
        DomainFinalidade.Normal => FinalidadeNFe.fnNormal,
        DomainFinalidade.Complementar => FinalidadeNFe.fnComplementar,
        DomainFinalidade.Ajuste => FinalidadeNFe.fnAjuste,
        DomainFinalidade.Devolucao => FinalidadeNFe.fnDevolucao,
        _ => throw new ArgumentException($"Finalidade invalida: {finalidade}")
    };

    private static PresencaComprador MapearPresenca(DomainPresenca presenca) => presenca switch
    {
        DomainPresenca.NaoSeAplica => PresencaComprador.pcNao,
        DomainPresenca.Presencial => PresencaComprador.pcPresencial,
        DomainPresenca.InternetOuTelefone => PresencaComprador.pcInternet,
        DomainPresenca.Teleatendimento => PresencaComprador.pcTeleatendimento,
        DomainPresenca.EntregaEmDomicilio => PresencaComprador.pcEntregaDomicilio,
        DomainPresenca.PresencialForaDoEstabelecimento => PresencaComprador.pcPresencialForaEstabelecimento,
        _ => PresencaComprador.pcOutros
    };

    private static ModalidadeFrete MapearModalidadeFrete(DomainModalidadeFrete modalidade) => modalidade switch
    {
        DomainModalidadeFrete.ContratacaoPorContaDoRemetente => ModalidadeFrete.mfContaEmitenteOumfContaRemetente,
        DomainModalidadeFrete.ContratacaoPorContaDoDestinatario => ModalidadeFrete.mfContaDestinatario,
        DomainModalidadeFrete.ContratacaoPorContaDeTerceiros => ModalidadeFrete.mfContaTerceiros,
        DomainModalidadeFrete.TransporteProprioPorContaDoRemetente => ModalidadeFrete.mfProprioContaRemente,
        DomainModalidadeFrete.TransporteProprioPorContaDoDestinatario => ModalidadeFrete.mfProprioContaDestinatario,
        _ => ModalidadeFrete.mfSemFrete
    };

    private static indIEDest MapearIndicadorIe(DomainIndicadorIe indicador) => indicador switch
    {
        DomainIndicadorIe.Contribuinte => indIEDest.ContribuinteICMS,
        DomainIndicadorIe.IsentoDeInscricao => indIEDest.Isento,
        DomainIndicadorIe.NaoContribuinte => indIEDest.NaoContribuinte,
        _ => throw new ArgumentException($"Indicador de IE invalido: {indicador}")
    };
}
