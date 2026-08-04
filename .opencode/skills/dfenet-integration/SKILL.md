---
name: dfenet-integration
description: Valida a integracao com Zeus.Net.NFe.NFCe (DFe.NET) — montagem XML, assinatura, comunicacao SEFAZ e DANFE.
---

Use esta skill quando houver alteracao no `DFeNetAdapter`, `CertificateReader`, ou `QuestPdfDanfeGenerator`.

## Checklist DFe.NET

1. **Montagem do XML**
   - [ ] `infNFe.versao` = "4.00"
   - [ ] `ide.mod` = `ModeloDocumento.NFCe` (65)
   - [ ] `ide.tpImp` = `TipoImpressao.DanfeNFCe`
   - [ ] `ide.indFinal` = `ConsumidorFinal.cfConsumidorFinal`
   - [ ] `ide.indPres` = `PresencaComprador.pcPresencial`
   - [ ] `ide.verProc` preenchido
   - [ ] `ide.dhEmi` com DateTimeOffset.Now

2. **Emitente**
   - [ ] CNPJ, razao social, IE, CRT preenchidos
   - [ ] Endereco completo com codigo IBGE

3. **Itens**
   - [ ] NCM, CFOP, CSOSN/CST preenchidos
   - [ ] Unidade comercial e tributavel
   - [ ] GTIN ou "SEM GTIN"
   - [ ] Origem da mercadoria (0-8)

4. **Pagamentos**
   - [ ] tPag mapeado: Dinheiro=01, Credito=03, Debito=04, PIX=17, Boleto=15
   - [ ] Soma dos pagamentos = total da nota
   - [ ] Troco calculado quando aplicavel

5. **Certificado**
   - [ ] PFX carregado com `X509KeyStorageFlags.MachineKeySet | Exportable`
   - [ ] Validade verificada antes do uso
   - [ ] Senha nunca logada

6. **Comunicacao SEFAZ**
   - [ ] `ConfiguracaoServico` com versao correta (4.00)
   - [ ] `TipoAmbiente` correto (Producao/Homologacao)
   - [ ] Tratamento de rejeicao (cStat != 100)
   - [ ] Tratamento de timeout/indisponibilidade

7. **DANFE NFC-e**
   - [ ] QuestPDF License = Community
   - [ ] QR Code gerado com QRCoder
   - [ ] Layout termica 80mm
   - [ ] Chave de acesso legivel

Referencias:
- NuGet: `Zeus.Net.NFe.NFCe` versao `2026.7.16.1250`
- NuGet: `Zeus.Net.NFe.Danfe.QuestPdf`
- GitHub: https://github.com/ZeusAutomacao/DFe.NET
