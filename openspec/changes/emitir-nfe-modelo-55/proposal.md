## Why

O motor só emite NFC-e (modelo 65). A NF-e (modelo 55) é requisito de lançamento
da plataforma, e ela não é "a mesma coisa com outro número" — muda o que é
obrigatório, muda o público e muda o que o destinatário faz com o documento.

Diferenças que o motor precisa passar a tratar:

- **Destinatário é obrigatório e completo**, com endereço e `indIEDest`. Na NFC-e
  o consumidor é opcional.
- **Grupos que a NFC-e não usa**: transporte (`transp`), volumes (`vol`),
  cobrança e duplicatas (`cobr`), documento referenciado (`refNFe`).
- **`tpNF`** distingue entrada de saída; **`finNFe`** distingue normal,
  complementar, ajuste e devolução.
- **Operação interestadual** com DIFAL, partilha e FCP.
- **Sem CSC e sem QR Code.** O CSC é da NFC-e; a NF-e não tem QR de consulta
  nesse formato. Mandar CSC numa NF-e é erro de contrato.
- **DANFE em retrato**, com layout próprio — não é o cupom da NFC-e.

## What Changes

- **Rotas novas** `POST /api/nfe/emit`, `/cancel`, `/consultar` e `/danfe`,
  espelhando as de NFC-e sem reaproveitar o DTO — os contratos divergem demais
  para caber num só.
- **`EmitirNfeRequest`** com destinatário obrigatório, `tpNF`, `finNFe`,
  `naturezaOperacao`, `refNFe`, transporte, volumes e cobrança.
- **Reaproveita o bloco tributário do item** criado na etapa 1, inclusive DIFAL,
  partilha e FCP, que na NFC-e não apareciam.
- **CFOP interestadual** (`6xxx`) aceito — a restrição a `5xxx` era da NFC-e.
- **DANFE modelo 55** em retrato, via QuestPDF, como documento próprio.
- **Validação recusa CSC em requisição de NF-e**, em vez de ignorar em silêncio.

## Capabilities

### New Capabilities
- `nfe-engine`: emissão, cancelamento, consulta e DANFE da NF-e modelo 55.

### Modified Capabilities
- `fiscal-engine-taxation`: o quadro tributário passa a cobrir DIFAL, partilha e
  FCP, exigidos por operação interestadual.

## Impact

- `FiscalService.Application/UseCases/EmitirNfe/` — caso de uso novo, com
  validador próprio.
- `FiscalService.Domain/` — value objects de destinatário completo, transporte,
  volume e cobrança.
- `FiscalService.Infrastructure/DFe/` — montagem do modelo 55; a DFe.NET já
  suporta, o trabalho é de adapter.
- **Depende da etapa 1**: sem o bloco tributário no item, NF-e não tem como
  destacar imposto — e destacar imposto é o ponto da NF-e.
- **Revisar antes de implementar.** Esta proposta foi escrita antes de a etapa 1
  existir; o contrato do item vai ensinar coisas que provavelmente mudam
  detalhes daqui. Ver o aviso no roteiro fiscal.
- Changes irmãs no backend e no frontend.
- Etapa **3** do roteiro fiscal (`gestao_fiscal_backend/ROADMAP_FISCAL.md`).
