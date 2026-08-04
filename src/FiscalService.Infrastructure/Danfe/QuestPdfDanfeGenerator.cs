using FiscalService.Application.Interfaces;
using NFe.Danfe.QuestPdf.ImpressaoNfce;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FiscalService.Infrastructure.Danfe;

/// <summary>
/// Gera o DANFE NFC-e usando o layout pronto da Zeus (NFe.Danfe.QuestPdf), que monta o
/// cupom a partir do proprio XML autorizado — inclusive o QR Code e a chave de acesso.
/// </summary>
public class QuestPdfDanfeGenerator : IDanfeGenerator
{
    private const TamanhoImpressao TamanhoBobina = TamanhoImpressao.Impressao80;

    static QuestPdfDanfeGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarDanfe(string xmlAutorizado, byte[]? logo = null)
    {
        if (string.IsNullOrWhiteSpace(xmlAutorizado))
            throw new ArgumentException("XML autorizado nao pode ser vazio", nameof(xmlAutorizado));

        var documento = new DanfeNfceDocument(xmlAutorizado, logo);
        documento.TamanhoImpressao(TamanhoBobina);

        return documento.GeneratePdf();
    }
}
