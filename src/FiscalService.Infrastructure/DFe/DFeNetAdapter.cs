using System.Security.Cryptography.X509Certificates;
using System.Text;
using FiscalService.Application.Interfaces;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;
using FiscalService.Domain.Exceptions;
using FiscalService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NFe.Classes;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Servicos.Tipos;
using NFe.Servicos;
using NFe.Utils;
using NFe.Utils.ConfiguracaoServico;

namespace FiscalService.Infrastructure.DFe;

public class DFeNetAdapter : IFiscalEngine
{
    private readonly ILogger<DFeNetAdapter> _logger;

    public DFeNetAdapter(ILogger<DFeNetAdapter> logger)
    {
        _logger = logger;
    }

    public async Task<EmitirNfceResultado> Emitir(
        Nfce nfce,
        Certificado certificado,
        Csc csc,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            var cert = LerCertificado(certificado);
            var configServico = CriarConfiguracaoServico(ambiente, cert, nfce.Itens.First().Cfop.Substring(0, 2));

            var nfe = MontarNfe(nfce, csc);
            var servicoNfe = new ServicosNFe(configServico);

            var retorno = servicoNfe.NfeAutorizacao(
                1,
                IndicadorSincronizacao.Assincrono,
                new[] { nfe },
                false);

            if (retorno.RetEnvio?.protNFe?.Count > 0)
            {
                var protocolo = retorno.RetEnvio.protNFe.First();

                if (protocolo.infProt.cStat == 100)
                {
                    var xmlAutorizado = retorno.RetEnvio.XmlRetorno;
                    var qrCode = MontarQrCode(nfce, csc, ambiente);

                    return new EmitirNfceResultado(
                        Sucesso: true,
                        ChaveAcesso: protocolo.infProt.chNFe,
                        Protocolo: protocolo.infProt.nProt,
                        XmlAutorizado: xmlAutorizado,
                        DanfePdf: null,
                        QrCode: qrCode,
                        MotivoRejeicao: null,
                        CodigoRejeicao: null
                    );
                }

                return new EmitirNfceResultado(
                    Sucesso: false,
                    ChaveAcesso: null,
                    Protocolo: null,
                    XmlAutorizado: null,
                    DanfePdf: null,
                    QrCode: null,
                    MotivoRejeicao: protocolo.infProt.xMotivo,
                    CodigoRejeicao: protocolo.infProt.cStat.ToString()
                );
            }

            return new EmitirNfceResultado(
                Sucesso: false,
                ChaveAcesso: null,
                Protocolo: null,
                XmlAutorizado: null,
                DanfePdf: null,
                QrCode: null,
                MotivoRejeicao: retorno.RetEnvio?.xMotivo ?? "Erro desconhecido na comunicacao com SEFAZ",
                CodigoRejeicao: retorno.RetEnvio?.cStat.ToString() ?? "000"
            );
        }
        catch (FiscalRejectionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir NFC-e");
            return new EmitirNfceResultado(
                Sucesso: false,
                ChaveAcesso: null,
                Protocolo: null,
                XmlAutorizado: null,
                DanfePdf: null,
                QrCode: null,
                MotivoRejeicao: ex.Message,
                CodigoRejeicao: "ERRO"
            );
        }
    }

    public async Task<ConsultarNfceResultado> Consultar(
        string chaveAcesso,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            var cert = LerCertificado(certificado);
            var uf = chaveAcesso.Substring(0, 2);
            var configServico = CriarConfiguracaoServico(ambiente, cert, uf);
            var servicoNfe = new ServicosNFe(configServico);

            var retorno = servicoNfe.NfeConsultaProtocolo(chaveAcesso);

            var sucesso = retorno.Retorno?.cStat == 100;

            return new ConsultarNfceResultado(
                Sucesso: sucesso,
                Status: retorno.Retorno?.xMotivo ?? "Desconhecido",
                Protocolo: retorno.Retorno?.protNFe?.infProt?.nProt,
                XmlConsulta: retorno.Retorno?.XmlRetorno
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar NFC-e {ChaveAcesso}", chaveAcesso);
            return new ConsultarNfceResultado(
                Sucesso: false,
                Status: ex.Message,
                Protocolo: null,
                XmlConsulta: null
            );
        }
    }

    public async Task<CancelarNfceResultado> Cancelar(
        string chaveAcesso,
        string protocolo,
        string justificativa,
        Certificado certificado,
        Ambiente ambiente,
        CancellationToken ct = default)
    {
        try
        {
            var cert = LerCertificado(certificado);
            var uf = chaveAcesso.Substring(0, 2);
            var configServico = CriarConfiguracaoServico(ambiente, cert, uf);
            var servicoNfe = new ServicosNFe(configServico);

            var retorno = servicoNfe.RecepcaoEventoCancelamento(
                1,
                chaveAcesso,
                protocolo,
                justificativa,
                1);

            var sucesso = retorno.Retorno?.infEvento?.cStat == 135
                || retorno.Retorno?.infEvento?.cStat == 155;

            return new CancelarNfceResultado(
                Sucesso: sucesso,
                Protocolo: retorno.Retorno?.infEvento?.nProt,
                XmlCancelamento: retorno.Retorno?.XmlRetorno,
                MotivoRejeicao: sucesso ? null : retorno.Retorno?.infEvento?.xMotivo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar NFC-e {ChaveAcesso}", chaveAcesso);
            return new CancelarNfceResultado(
                Sucesso: false,
                Protocolo: null,
                XmlCancelamento: null,
                MotivoRejeicao: ex.Message
            );
        }
    }

    public async Task<StatusServicoResultado> StatusServico(
        Ambiente ambiente,
        string uf,
        CancellationToken ct = default)
    {
        try
        {
            var configServico = CriarConfiguracaoServicoSemCertificado(ambiente, uf);
            var servicoNfe = new ServicosNFe(configServico);

            var retorno = servicoNfe.NfeStatusServico();

            var disponivel = retorno.Retorno?.cStat == 107;

            return new StatusServicoResultado(
                Disponivel: disponivel,
                Mensagem: retorno.Retorno?.xMotivo ?? "Erro ao consultar status",
                TempoMedioResposta: (int?)retorno.Retorno?.tMed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status do servico SEFAZ");
            return new StatusServicoResultado(
                Disponivel: false,
                Mensagem: ex.Message,
                TempoMedioResposta: null
            );
        }
    }

    private X509Certificate2 LerCertificado(Certificado certificado)
    {
        try
        {
            var pfxBytes = Convert.FromBase64String(certificado.PfxBase64);
            var cert = new X509Certificate2(pfxBytes, certificado.Senha,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

            if (cert.NotAfter < DateTime.UtcNow)
            {
                throw new InvalidCertificateException(
                    $"Certificado vencido em {cert.NotAfter:dd/MM/yyyy}");
            }

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

    private ConfiguracaoServico CriarConfiguracaoServico(
        Ambiente ambiente,
        X509Certificate2 cert,
        string uf)
    {
        var tipoAmbiente = ambiente == Ambiente.Producao
            ? TipoAmbiente.taProducao
            : TipoAmbiente.taHomologacao;

        var estadoUf = (Estado)Enum.Parse(typeof(Estado), uf);

        return new ConfiguracaoServico
        {
            cUF = estadoUf,
            tpAmb = tipoAmbiente,
            VersaoNfeAutorizacao = VersaoServico.Versao400,
            VersaoNfeConsultaProtocolo = VersaoServico.Versao400,
            VersaoNfeStatusServico = VersaoServico.Versao400,
            VersaoRecepcaoEventoCceCancelamento = VersaoServico.Versao400,
            CertificadoDigital = new ConfiguracaoCertificado
            {
                ArrayBytesArquivo = cert.Export(X509ContentType.Pfx),
                Senha = string.Empty
            }
        };
    }

    private ConfiguracaoServico CriarConfiguracaoServicoSemCertificado(
        Ambiente ambiente,
        string uf)
    {
        var tipoAmbiente = ambiente == Ambiente.Producao
            ? TipoAmbiente.taProducao
            : TipoAmbiente.taHomologacao;

        var estadoUf = (Estado)Enum.Parse(typeof(Estado), uf);

        return new ConfiguracaoServico
        {
            cUF = estadoUf,
            tpAmb = tipoAmbiente,
            VersaoNfeStatusServico = VersaoServico.Versao400
        };
    }

    private global::NFe.Classes.NFe MontarNfe(Nfce nfce, Csc csc)
    {
        var nfe = new global::NFe.Classes.NFe
        {
            infNFe = new infNFe
            {
                versao = "4.00",
                ide = new ide
                {
                    mod = ModeloDocumento.NFCe,
                    serie = nfce.Serie,
                    nNF = nfce.Numero,
                    tpAmb = nfce.Ambiente == Ambiente.Producao
                        ? TipoAmbiente.taProducao
                        : TipoAmbiente.taHomologacao,
                    tpNF = TipoNFe.tnSaida,
                    tpEmis = TipoEmissao.teNormal,
                    tpImp = TipoImpressao.DanfeNFCe,
                    cNF = nfce.Numero.ToString().PadLeft(8, '0'),
                    dhEmi = DateTimeOffset.Now,
                    dhSaiEnt = null,
                    finNFe = FinalidadeNFe.fnNormal,
                    indFinal = ConsumidorFinal.cfConsumidorFinal,
                    indPres = PresencaComprador.pcPresencial,
                    verProc = "1.0.0"
                },
                emit = new emit(),
                dest = new dest(),
                det = nfce.Itens.Select(i => new det
                {
                    nItem = i.NumeroItem,
                    prod = new prod
                    {
                        cProd = i.CodigoProduto,
                        cEAN = i.Gtin ?? "SEM GTIN",
                        xProd = i.Descricao,
                        NCM = i.Ncm,
                        CEST = i.Cest,
                        CFOP = i.Cfop,
                        uCom = i.UnidadeComercial,
                        qCom = i.Quantidade,
                        vUnCom = i.ValorUnitario,
                        vProd = i.ValorTotal,
                        cEANTrib = i.Gtin ?? "SEM GTIN",
                        uTrib = i.UnidadeComercial,
                        qTrib = i.Quantidade,
                        vUnTrib = i.ValorUnitario,
                        indTot = IndicadorTotal.itCompoeTotalNF
                    },
                    imposto = new imposto
                    {
                        ICMS = new ICMS
                        {
                            TipoICMS = new ICMS00
                            {
                                orig = (OrigemMercadoria)i.Origem,
                                CST = "00",
                                modBC = DeterminacaoBaseIcms.dbiValorOperacao,
                                vBC = 0,
                                pICMS = 0,
                                vICMS = 0
                            }
                        },
                        PIS = new PIS
                        {
                            TipoPIS = new PISQtde { CST = "01", qBCProd = 0, vAliqProd = 0, vPIS = 0 }
                        },
                        COFINS = new COFINS
                        {
                            TipoCOFINS = new COFINSQtde { CST = "01", qBCProd = 0, vAliqProd = 0, vCOFINS = 0 }
                        }
                    }
                }).ToList(),
                total = new total
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
                        vProd = nfce.Itens.Sum(i => i.ValorTotal),
                        vFrete = 0,
                        vSeg = 0,
                        vDesc = 0,
                        vII = 0,
                        vIPI = 0,
                        vIPIDevol = 0,
                        vPIS = 0,
                        vCOFINS = 0,
                        vOutro = 0,
                        vNF = nfce.Itens.Sum(i => i.ValorTotal),
                        vTotTrib = 0
                    }
                },
                transp = new transp { modFrete = ModalidadeFrete.mfSemOcorrenciaTransporte },
                pag = new pag
                {
                    detPag = nfce.Pagamentos.Select(p => new detPag
                    {
                        tPag = MapearTipoPagamento(p.Tipo),
                        vPag = p.Valor
                    }).ToList()
                }
            }
        };

        return nfe;
    }

    private FormaPagamento MapearTipoPagamento(TipoPagamento tipo)
    {
        return tipo switch
        {
            TipoPagamento.Dinheiro => FormaPagamento.fpDinheiro,
            TipoPagamento.CartaoCredito => FormaPagamento.fpCartaoCredito,
            TipoPagamento.CartaoDebito => FormaPagamento.fpCartaoDebito,
            TipoPagamento.Pix => FormaPagamento.fpPix,
            TipoPagamento.BoletoBancario => FormaPagamento.fpBoleto,
            TipoPagamento.ValeAlimentacao => FormaPagamento.fpValeAlimentacao,
            TipoPagamento.ValeRefeicao => FormaPagamento.fpValeRefeicao,
            TipoPagamento.ValePresente => FormaPagamento.fpValePresente,
            TipoPagamento.SemPagamento => FormaPagamento.fpSemPagamento,
            _ => FormaPagamento.fpOutro
        };
    }

    private string MontarQrCode(Nfce nfce, Csc csc, Ambiente ambiente)
    {
        var ambienteStr = ambiente == Ambiente.Producao ? "1" : "2";
        return $"https://www.sefaz.br.gov.br/nfce/consulta?p=0|{nfce.ChaveAcesso ?? string.Empty}|{ambienteStr}|0|0";
    }
}
