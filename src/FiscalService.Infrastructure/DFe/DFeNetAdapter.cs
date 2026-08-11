using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Classes.Servicos.Tipos;
using NFe.Servicos;
using NFe.Utils;
using NFe.Utils.InformacoesSuplementares;
using NFe.Utils.NFe;
using Ambiente = FiscalService.Domain.Enums.Ambiente;
using DomainCrt = FiscalService.Domain.Enums.Crt;
using NFeClasse = NFe.Classes.NFe;
using TipoPagamento = FiscalService.Domain.Enums.TipoPagamento;

namespace FiscalService.Infrastructure.DFe;

/// <summary>
/// Adapter sobre a Zeus.Net.NFe (DFe.NET) para emissao, consulta e cancelamento de NFC-e.
///
/// Importante: nada aqui usa <c>ConfiguracaoServico.Instancia</c> (o singleton estatico da
/// biblioteca). Toda configuracao e criada por request e passada explicitamente, porque o
/// servico e stateless e atende chamadas concorrentes com certificados diferentes.
/// </summary>
public class DFeNetAdapter : IFiscalEngine
{
    private const string VersaoLayoutNfe = "4.00";
    private const string VersaoAplicativo = "FiscalService 1.0";
    private const VersaoServico VersaoServicoNfe = VersaoServico.Versao400;
    private const VersaoQrCode VersaoQrCodeNfce = VersaoQrCode.QrCodeVersao2;

    /// <summary>
    /// Texto obrigatorio no nome do destinatario em ambiente de homologacao (NT 2015/002).
    /// </summary>
    private const string NomeDestinatarioHomologacao =
        "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

    /// <summary>
    /// Texto obrigatorio na descricao do primeiro item em ambiente de homologacao
    /// (NT 2015/002). Sem ele a SEFAZ devolve a rejeicao 373.
    /// </summary>
    private const string DescricaoPrimeiroItemHomologacao =
        "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

    private readonly ILogger<DFeNetAdapter> _logger;

    public DFeNetAdapter(ILogger<DFeNetAdapter> logger)
    {
        _logger = logger;
    }

    public Task<EmitirNfceResultado> Emitir(
        Nfce nfce,
        Certificado certificado,
        Csc csc,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            var uf = ParseEstado(nfce.Emitente.Endereco.Uf);
            using var cert = LerCertificado(certificado);
            var configuracao = CriarConfiguracao(ambiente, uf, certificado);

            var nfe = MontarNfe(nfce, ambiente, uf);
            AplicarInformacoesSuplementares(nfe, csc, configuracao.Certificado);

            nfe = nfe.Assina(configuracao, cert);

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.NFeAutorizacao(
                idLote: 1,
                indSinc: IndicadorSincronizacao.Sincrono,
                nFes: new List<NFeClasse> { nfe },
                compactarMensagem: false);

            var protocolo = retorno.Retorno?.protNFe?.infProt;

            if (protocolo is null)
            {
                // Lote rejeitado antes de gerar protocolo (ex.: schema, certificado, CSC invalido).
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
                // O qrCode carrega apenas chave, versao, tpAmb, cIdToken e o hash — nenhum
                // segredo. E o unico jeito de diagnosticar a rejeicao 464 sem o CSC em log.
                _logger.LogWarning("NFC-e rejeitada pela SEFAZ: {CStat} - {Motivo} | qrCode: {QrCode}",
                    protocolo.cStat, protocolo.xMotivo, nfe.infNFeSupl?.qrCode);

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
                NFe = nfe,
                protNFe = retorno.Retorno!.protNFe
            };

            return Task.FromResult(new EmitirNfceResultado(
                Sucesso: true,
                ChaveAcesso: protocolo.chNFe,
                Protocolo: protocolo.nProt,
                XmlAutorizado: FuncoesXml.ClasseParaXmlString(proc),
                QrCode: nfe.infNFeSupl?.qrCode,
                MotivoRejeicao: null,
                CodigoRejeicao: null));
        }
        catch (InvalidCertificateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir NFC-e serie {Serie} numero {Numero}", nfce.Serie, nfce.Numero);
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

    public Task<ConsultarNfceResultado> Consultar(
        string chaveAcesso,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            using var cert = LerCertificado(certificado);
            var configuracao = CriarConfiguracao(ambiente, UfDaChave(chaveAcesso), certificado);

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.NfeConsultaProtocolo(chaveAcesso);
            var consulta = retorno.Retorno;

            return Task.FromResult(new ConsultarNfceResultado(
                Sucesso: consulta?.cStat == 100,
                Status: consulta?.xMotivo ?? "Resposta vazia da SEFAZ",
                Protocolo: consulta?.protNFe?.infProt?.nProt,
                XmlConsulta: retorno.RetornoStr));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar NFC-e {ChaveAcesso}", chaveAcesso);
            return Task.FromResult(new ConsultarNfceResultado(
                Sucesso: false,
                Status: ex.Message,
                Protocolo: null,
                XmlConsulta: null));
        }
    }

    public Task<CancelarNfceResultado> Cancelar(
        string chaveAcesso,
        string protocolo,
        string justificativa,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            using var cert = LerCertificado(certificado);
            var configuracao = CriarConfiguracao(ambiente, UfDaChave(chaveAcesso), certificado);

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.RecepcaoEventoCancelamento(
                idlote: 1,
                sequenciaEvento: 1,
                protocoloAutorizacao: protocolo,
                chaveNFe: chaveAcesso,
                justificativa: justificativa,
                cpfcnpj: CnpjDaChave(chaveAcesso),
                dhEvento: null);

            var evento = retorno.Retorno?.retEvento?.FirstOrDefault()?.infEvento;

            // 135 = evento registrado e vinculado a NF-e; 155 = cancelamento homologado fora do prazo.
            var sucesso = evento?.cStat == 135 || evento?.cStat == 155;

            var xmlCancelamento = retorno.ProcEventosNFe?.Count > 0
                ? FuncoesXml.ClasseParaXmlString(retorno.ProcEventosNFe[0])
                : retorno.RetornoStr;

            return Task.FromResult(new CancelarNfceResultado(
                Sucesso: sucesso,
                Protocolo: evento?.nProt,
                XmlCancelamento: xmlCancelamento,
                MotivoRejeicao: sucesso
                    ? null
                    : evento?.xMotivo ?? retorno.Retorno?.xMotivo ?? "Resposta vazia da SEFAZ"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar NFC-e {ChaveAcesso}", chaveAcesso);
            return Task.FromResult(new CancelarNfceResultado(
                Sucesso: false,
                Protocolo: null,
                XmlCancelamento: null,
                MotivoRejeicao: ex.Message));
        }
    }

    public Task<StatusServicoResultado> StatusServico(
        Ambiente ambiente,
        string uf,
        Certificado certificado,
        CancellationToken ct = default)
    {
        try
        {
            using var cert = LerCertificado(certificado);
            var configuracao = CriarConfiguracao(ambiente, ParseEstado(uf), certificado);

            using var servico = new ServicosNFe(configuracao, cert);
            var retorno = servico.NfeStatusServico(exceptionCompleta: false);
            var status = retorno.Retorno;

            return Task.FromResult(new StatusServicoResultado(
                Disponivel: status?.cStat == 107,
                Mensagem: status?.xMotivo ?? "Resposta vazia da SEFAZ",
                TempoMedioResposta: status?.tMed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status do servico SEFAZ da UF {Uf}", uf);
            return Task.FromResult(new StatusServicoResultado(
                Disponivel: false,
                Mensagem: ex.Message,
                TempoMedioResposta: null));
        }
    }

    // ---------------------------------------------------------------- certificado / configuracao

    private static X509Certificate2 LerCertificado(Certificado certificado)
    {
        try
        {
            // Exportable sem MachineKeySet: funciona tanto no container Linux quanto no
            // Windows de desenvolvimento, e nao exige permissao de escrita no store da maquina.
            var cert = new X509Certificate2(
                Convert.FromBase64String(certificado.PfxBase64),
                certificado.Senha,
                X509KeyStorageFlags.Exportable);

            if (cert.NotAfter < DateTime.Now)
                throw new InvalidCertificateException($"Certificado vencido em {cert.NotAfter:dd/MM/yyyy}");

            if (!cert.HasPrivateKey)
                throw new InvalidCertificateException("Certificado nao possui chave privada");

            return cert;
        }
        catch (InvalidCertificateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidCertificateException("Falha ao ler certificado PFX", ex);
        }
    }

    private static ConfiguracaoServico CriarConfiguracao(Ambiente ambiente, Estado uf, Certificado certificado)
    {
        return new ConfiguracaoServico
        {
            cUF = uf,
            tpAmb = ambiente == Ambiente.Producao ? TipoAmbiente.Producao : TipoAmbiente.Homologacao,
            tpEmis = TipoEmissao.teNormal,
            ModeloDocumento = ModeloDocumento.NFCe,
            VersaoLayout = VersaoServicoNfe,
            VersaoNFeAutorizacao = VersaoServicoNfe,
            VersaoNFeRetAutorizacao = VersaoServicoNfe,
            VersaoNfeConsultaProtocolo = VersaoServicoNfe,
            VersaoNfeStatusServico = VersaoServicoNfe,
            VersaoRecepcaoEventoCceCancelamento = VersaoServicoNfe,
            // Sem schemas XSD embarcados no container, a validacao local nao tem como rodar;
            // quem valida e a propria SEFAZ.
            ValidarSchemas = false,
            SalvarXmlServicos = false,
            Certificado = new ConfiguracaoCertificado
            {
                TipoCertificado = TipoCertificado.A1ByteArray,
                ArrayBytesArquivo = Convert.FromBase64String(certificado.PfxBase64),
                Senha = certificado.Senha,
                ManterDadosEmCache = false
            }
        };
    }

    // ---------------------------------------------------------------- montagem da NFe

    private static NFeClasse MontarNfe(Nfce nfce, Ambiente ambiente, Estado uf)
    {
        if (nfce.Itens.Count == 0)
            throw new ArgumentException("NFC-e sem itens");

        if (nfce.Pagamentos.Count == 0)
            throw new ArgumentException("NFC-e sem formas de pagamento");

        var emitente = nfce.Emitente;
        var codigoNumerico = GerarCodigoNumerico();
        var tpAmb = ambiente == Ambiente.Producao ? TipoAmbiente.Producao : TipoAmbiente.Homologacao;

        var chave = ChaveFiscal.ObterChave(
            ufEmitente: uf,
            dataEmissao: nfce.DataEmissao,
            cnpjEmitente: emitente.Cnpj.Valor,
            modelo: ModeloDocumento.NFCe,
            serie: nfce.Serie,
            numero: nfce.Numero,
            tipoEmissao: (int)TipoEmissao.teNormal,
            cNf: int.Parse(codigoNumerico));

        var nfe = new NFeClasse
        {
            infNFe = new infNFe
            {
                versao = VersaoLayoutNfe,
                Id = $"NFe{chave.Chave}",
                ide = new ide
                {
                    cUF = uf,
                    cNF = codigoNumerico,
                    natOp = nfce.NaturezaOperacao,
                    mod = ModeloDocumento.NFCe,
                    serie = nfce.Serie,
                    nNF = nfce.Numero,
                    dhEmi = nfce.DataEmissao,
                    tpNF = TipoNFe.tnSaida,
                    idDest = DestinoOperacao.doInterna,
                    cMunFG = long.Parse(emitente.Endereco.CodigoMunicipio),
                    tpImp = TipoImpressao.tiNFCe,
                    tpEmis = TipoEmissao.teNormal,
                    cDV = chave.DigitoVerificador,
                    tpAmb = tpAmb,
                    finNFe = FinalidadeNFe.fnNormal,
                    indFinal = ConsumidorFinal.cfConsumidorFinal,
                    indPres = PresencaComprador.pcPresencial,
                    procEmi = ProcessoEmissao.peAplicativoContribuinte,
                    verProc = VersaoAplicativo
                },
                emit = MontarEmitente(emitente, uf),
                dest = MontarDestinatario(nfce, ambiente),
                det = nfce.Itens
                    .Select((item, indice) => MontarDetalhe(item, emitente.Crt, ambiente, indice == 0))
                    .ToList(),
                total = MontarTotal(nfce),
                transp = new transp { modFrete = ModalidadeFrete.mfSemFrete },
                pag = new List<pag>
                {
                    new()
                    {
                        detPag = nfce.Pagamentos
                            .Select(p => new detPag
                            {
                                indPag = IndicadorPagamentoDetalhePagamento.ipDetPgVista,
                                tPag = MapearFormaPagamento(p.Tipo),
                                vPag = p.Valor
                            })
                            .ToList()
                    }
                }
            }
        };

        return nfe;
    }

    private static emit MontarEmitente(Emitente emitente, Estado uf)
    {
        return new emit
        {
            CNPJ = emitente.Cnpj.Valor,
            xNome = emitente.RazaoSocial,
            xFant = emitente.NomeFantasia,
            IE = emitente.InscricaoEstadual,
            CRT = MapearCrt(emitente.Crt),
            enderEmit = new enderEmit
            {
                xLgr = emitente.Endereco.Logradouro,
                nro = emitente.Endereco.Numero,
                xCpl = emitente.Endereco.Complemento,
                xBairro = emitente.Endereco.Bairro,
                cMun = long.Parse(emitente.Endereco.CodigoMunicipio),
                xMun = emitente.Endereco.Municipio,
                UF = uf,
                CEP = emitente.Endereco.Cep,
                cPais = 1058,
                xPais = "BRASIL",
                fone = ParseTelefone(emitente.Telefone)
            }
        };
    }

    /// <summary>
    /// Na NFC-e o consumidor e opcional. Quando identificado, em homologacao o nome
    /// e obrigatoriamente substituido pelo texto padrao da NT 2015/002.
    /// </summary>
    private static dest? MontarDestinatario(Nfce nfce, Ambiente ambiente)
    {
        var destinatario = nfce.Destinatario;

        if (destinatario is null || !destinatario.Identificado)
            return null;

        var dest = new dest(VersaoServicoNfe)
        {
            xNome = ambiente == Ambiente.Homologacao
                ? NomeDestinatarioHomologacao
                : destinatario.Nome,
            indIEDest = indIEDest.NaoContribuinte
        };

        if (destinatario.Documento is not null)
        {
            if (destinatario.Documento.EhCnpj)
                dest.CNPJ = destinatario.Documento.Valor;
            else
                dest.CPF = destinatario.Documento.Valor;
        }

        return dest;
    }

    /// <summary>
    /// Em homologacao a descricao do primeiro item e obrigatoriamente substituida pelo texto
    /// padrao da NT 2015/002 — os demais itens mantem a descricao original.
    /// </summary>
    private static det MontarDetalhe(NfceItem item, DomainCrt crt, Ambiente ambiente, bool primeiroItem)
    {
        var descricao = ambiente == Ambiente.Homologacao && primeiroItem
            ? DescricaoPrimeiroItemHomologacao
            : item.Descricao;

        return new det
        {
            nItem = item.NumeroItem,
            prod = new prod
            {
                cProd = item.CodigoProduto,
                cEAN = string.IsNullOrWhiteSpace(item.Gtin) ? "SEM GTIN" : item.Gtin,
                xProd = descricao,
                NCM = item.Ncm,
                CEST = item.Cest,
                CFOP = int.Parse(item.Cfop),
                uCom = item.UnidadeComercial,
                qCom = item.Quantidade,
                vUnCom = item.ValorUnitario,
                vProd = item.ValorTotal,
                cEANTrib = string.IsNullOrWhiteSpace(item.Gtin) ? "SEM GTIN" : item.Gtin,
                uTrib = item.UnidadeComercial,
                qTrib = item.Quantidade,
                vUnTrib = item.ValorUnitario,
                indTot = IndicadorTotal.ValorDoItemCompoeTotalNF
            },
            imposto = MontarImposto(item, crt)
        };
    }

    /// <summary>
    /// O payload recebido do backend traz apenas origem e CSOSN/CST — nao traz base de
    /// calculo nem aliquotas. Por isso so sao aceitas as situacoes tributarias que se
    /// resolvem sem valores; as demais falham explicitamente em vez de emitir imposto zerado.
    /// </summary>
    private static imposto MontarImposto(NfceItem item, DomainCrt crt)
    {
        var origem = MapearOrigem(item.Origem);
        var codigo = (item.Csosn ?? string.Empty).Trim().PadLeft(3, '0');

        ICMSBasico icms = crt == DomainCrt.RegimeNormal
            ? MontarIcmsRegimeNormal(codigo, origem)
            : MontarIcmsSimplesNacional(codigo, origem);

        return new imposto
        {
            ICMS = new ICMS { TipoICMS = icms },
            // Sem dados de PIS/COFINS no payload, a unica emissao possivel sem inventar
            // base de calculo e a nao tributada (CST 07 - operacao isenta da contribuicao).
            PIS = new PIS { TipoPIS = new PISNT { CST = CSTPIS.pis07 } },
            COFINS = new COFINS { TipoCOFINS = new COFINSNT { CST = CSTCOFINS.cofins07 } }
        };
    }

    private static ICMSBasico MontarIcmsSimplesNacional(string csosn, OrigemMercadoria origem)
    {
        return csosn switch
        {
            "102" or "103" or "300" or "400" => new ICMSSN102
            {
                orig = origem,
                CSOSN = MapearCsosn(csosn)
            },
            "500" => new ICMSSN500
            {
                orig = origem,
                CSOSN = Csosnicms.Csosn500
            },
            _ => throw new ArgumentException(
                $"CSOSN {csosn} exige valores de base de calculo/aliquota que nao vem no payload. " +
                "Use 102, 103, 300, 400 ou 500.")
        };
    }

    private static ICMSBasico MontarIcmsRegimeNormal(string cst, OrigemMercadoria origem)
    {
        var codigo = cst.TrimStart('0').PadLeft(2, '0');

        return codigo switch
        {
            "40" or "41" or "50" => new ICMS40
            {
                orig = origem,
                CST = MapearCsticms(codigo)
            },
            _ => throw new ArgumentException(
                $"CST {codigo} exige valores de base de calculo/aliquota que nao vem no payload. " +
                "Use 40, 41 ou 50.")
        };
    }

    private static total MontarTotal(Nfce nfce)
    {
        var valorProdutos = nfce.Itens.Sum(i => i.ValorTotal);

        return new total
        {
            ICMSTot = new ICMSTot
            {
                vBC = 0,
                vICMS = 0,
                vICMSDeson = 0,
                vFCP = 0,
                vBCST = 0,
                vST = 0,
                vFCPST = 0,
                vFCPSTRet = 0,
                vProd = valorProdutos,
                vFrete = 0,
                vSeg = 0,
                vDesc = 0,
                vII = 0,
                vIPI = 0,
                vIPIDevol = 0,
                vPIS = 0,
                vCOFINS = 0,
                vOutro = 0,
                vNF = valorProdutos
            }
        };
    }

    private static void AplicarInformacoesSuplementares(NFeClasse nfe, Csc csc, ConfiguracaoCertificado certificado)
    {
        var supl = new infNFeSupl();

        // Precisa ser preenchido antes da assinatura: o QR Code faz parte do XML assinado.
        supl.qrCode = supl.ObterUrlQrCode(nfe, VersaoQrCodeNfce, csc.IdCsc, csc.Codigo, certificado);
        supl.urlChave = supl.ObterUrlConsulta(nfe, VersaoQrCodeNfce);

        nfe.infNFeSupl = supl;
    }

    // ---------------------------------------------------------------- mapeamentos

    private static FormaPagamento MapearFormaPagamento(TipoPagamento tipo) => tipo switch
    {
        TipoPagamento.Dinheiro => FormaPagamento.fpDinheiro,
        TipoPagamento.Cheque => FormaPagamento.fpCheque,
        TipoPagamento.CartaoCredito => FormaPagamento.fpCartaoCredito,
        TipoPagamento.CartaoDebito => FormaPagamento.fpCartaoDebito,
        TipoPagamento.CreditoLoja => FormaPagamento.fpCreditoEmLoja,
        TipoPagamento.ValeAlimentacao => FormaPagamento.fpValeAlimentacao,
        TipoPagamento.ValeRefeicao => FormaPagamento.fpValeRefeicao,
        TipoPagamento.ValePresente => FormaPagamento.fpValePresente,
        TipoPagamento.ValeCombustivel => FormaPagamento.fpValeCombustivel,
        TipoPagamento.BoletoBancario => FormaPagamento.fpBoletoBancario,
        TipoPagamento.Pix => FormaPagamento.fpPagamentoInstantaneoPIXDinamico,
        TipoPagamento.SemPagamento => FormaPagamento.fpSemPagamento,
        _ => FormaPagamento.fpOutro
    };

    private static CRT MapearCrt(DomainCrt crt) => crt switch
    {
        DomainCrt.SimplesNacional => CRT.SimplesNacional,
        DomainCrt.SimplesNacionalExcessoSublimite => CRT.SimplesNacionalExcessoSublimite,
        DomainCrt.RegimeNormal => CRT.RegimeNormal,
        DomainCrt.SimplesNacionalMei => CRT.SimplesNacionalMei,
        _ => throw new ArgumentException($"CRT invalido: {crt}")
    };

    private static OrigemMercadoria MapearOrigem(int origem) => origem switch
    {
        0 => OrigemMercadoria.OmNacional,
        1 => OrigemMercadoria.OmEstrangeiraImportacaoDireta,
        2 => OrigemMercadoria.OmEstrangeiraAdquiridaBrasil,
        3 => OrigemMercadoria.OmNacionalConteudoImportacaoSuperior40,
        4 => OrigemMercadoria.OmNacionalProcessosBasicos,
        5 => OrigemMercadoria.OmNacionalConteudoImportacaoInferiorIgual40,
        6 => OrigemMercadoria.OmEstrangeiraImportacaoDiretaSemSimilar,
        7 => OrigemMercadoria.OmEstrangeiraAdquiridaBrasilSemSimilar,
        8 => OrigemMercadoria.OmNacionalConteudoImportacaoSuperior70,
        _ => throw new ArgumentException($"Origem da mercadoria invalida: {origem}")
    };

    private static Csosnicms MapearCsosn(string csosn) => csosn switch
    {
        "102" => Csosnicms.Csosn102,
        "103" => Csosnicms.Csosn103,
        "300" => Csosnicms.Csosn300,
        "400" => Csosnicms.Csosn400,
        "500" => Csosnicms.Csosn500,
        _ => throw new ArgumentException($"CSOSN invalido: {csosn}")
    };

    private static Csticms MapearCsticms(string cst) => cst switch
    {
        "40" => Csticms.Cst40,
        "41" => Csticms.Cst41,
        "50" => Csticms.Cst50,
        _ => throw new ArgumentException($"CST invalido: {cst}")
    };

    private static Estado ParseEstado(string uf)
    {
        if (Enum.TryParse<Estado>(uf?.Trim().ToUpperInvariant(), out var estado))
            return estado;

        throw new ArgumentException($"UF invalida: {uf}");
    }

    /// <summary>Os 2 primeiros digitos da chave de acesso sao o codigo IBGE da UF.</summary>
    private static Estado UfDaChave(string chaveAcesso)
    {
        if (chaveAcesso?.Length != 44 || !int.TryParse(chaveAcesso[..2], out var codigoUf))
            throw new ArgumentException($"Chave de acesso invalida: {chaveAcesso}");

        if (!Enum.IsDefined(typeof(Estado), codigoUf))
            throw new ArgumentException($"UF invalida na chave de acesso: {chaveAcesso[..2]}");

        return (Estado)codigoUf;
    }

    /// <summary>Posicoes 7 a 20 da chave de acesso sao o CNPJ do emitente.</summary>
    private static string CnpjDaChave(string chaveAcesso)
    {
        if (chaveAcesso?.Length != 44)
            throw new ArgumentException($"Chave de acesso invalida: {chaveAcesso}");

        return chaveAcesso.Substring(6, 14);
    }

    private static long? ParseTelefone(string? telefone)
    {
        var digitos = new string((telefone ?? string.Empty).Where(char.IsDigit).ToArray());
        return digitos.Length is >= 6 and <= 14 ? long.Parse(digitos) : null;
    }

    /// <summary>
    /// cNF: codigo numerico de 8 digitos. A NT 2019/001 proibe que seja igual ao nNF,
    /// entao e sempre aleatorio.
    /// </summary>
    private static string GerarCodigoNumerico()
    {
        return RandomNumberGenerator.GetInt32(1, 99_999_999).ToString("D8");
    }
}
