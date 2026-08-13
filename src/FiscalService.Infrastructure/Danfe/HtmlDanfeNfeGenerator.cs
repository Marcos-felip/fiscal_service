using System.Text;
using DFe.Utils;
using FiscalService.Application.Interfaces;
using NFe.Classes;
using NFe.Danfe.Html;
using NFe.Danfe.Html.CrossCutting;
using NFe.Danfe.Html.Dominio;

namespace FiscalService.Infrastructure.Danfe;

/// <summary>
/// Gera o DANFE do modelo 55 em HTML, a partir do XML autorizado.
///
/// HTML e nao PDF por uma restricao medida, nao por preferencia: o layout retrato pronto da
/// Zeus vive no pacote PdfClown, que depende de <c>System.Drawing.Common</c> — Windows-only
/// no .NET 8, e o motor roda em contentor Linux. Ver D6 no design da change.
///
/// O DANFE e representacao grafica; o documento fiscal e o XML. Imprimir do navegador
/// resolve, sem Chromium na imagem e sem escrever o layout a mao.
/// </summary>
public class HtmlDanfeNfeGenerator : IDanfeNfeGenerator
{
    public DanfeGerado Gerar(string xmlAutorizado, byte[]? logo = null)
    {
        if (string.IsNullOrWhiteSpace(xmlAutorizado))
            throw new ArgumentException("XML autorizado nao pode ser vazio", nameof(xmlAutorizado));

        var proc = FuncoesXml.XmlStringParaClasse<nfeProc>(xmlAutorizado);

        var danfe = new DanfeNFe(
            proc.NFe,
            Status.Autorizada,
            proc.protNFe?.infProt?.nProt,
            creditos: null,
            issqn: null,
            logo: logo is null ? null : Convert.ToBase64String(logo));

        // A API e assincrona, mas nao faz I/O: monta a string em memoria. O handler que a
        // chama ja e async e nao ganha nada em propagar isso.
        var documento = new DanfeNfeHtml2(danfe).ObterDocHtmlAsync().GetAwaiter().GetResult();

        return new DanfeGerado(Encoding.UTF8.GetBytes(documento.Html), DanfeGerado.Html);
    }
}
