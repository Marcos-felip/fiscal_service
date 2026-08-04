using System.Text;
using FiscalService.Application.Interfaces;
using FiscalService.Domain.Entities;
using NFe.Classes;
using QuestPDF.Infrastructure;

namespace FiscalService.Infrastructure.Danfe;

public class QuestPdfDanfeGenerator : IDanfeGenerator
{
    static QuestPdfDanfeGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarDanfe(Nfce nfce, string qrCode, string chaveAcesso)
    {
        var xmlNfe = MontarXmlNfe(nfce, chaveAcesso);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlNfe));
        var proc = new nfeProc();
        proc.NFe = new global::NFe.Classes.NFe();

        try
        {
            var danfeNfce = new global::NFe.Danfe.QuestPdf.Nfce.DanfeNfce(
                proc,
                new global::NFe.Danfe.QuestPdf.Nfce.ConfiguracaoDanfeNfce(
                    global::NFe.Danfe.QuestPdf.Nfce.NfceDetalheVendaNormal.UmaLinha,
                    global::NFe.Danfe.QuestPdf.Nfce.NfceDetalheVendaContigencia.UmaLinha,
                    null));

            return danfeNfce.GerarPDF();
        }
        catch (Exception)
        {
            return GerarDanfeSimples(nfce, qrCode, chaveAcesso);
        }
    }

    private byte[] GerarDanfeSimples(Nfce nfce, string qrCode, string chaveAcesso)
    {
        return Encoding.UTF8.GetBytes(
            $"DANFE NFC-e\nChave: {chaveAcesso}\nSerie: {nfce.Serie}\nNumero: {nfce.Numero}\nQR: {qrCode}");
    }

    private string MontarXmlNfe(Nfce nfce, string chaveAcesso)
    {
        return $"<nfeProc><NFe><infNFe><ide><chNFe>{chaveAcesso}</chNFe></ide></infNFe></NFe></nfeProc>";
    }
}
