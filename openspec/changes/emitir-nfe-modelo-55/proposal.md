> **Podada em 13/08/2026**, contra o código real e contra os dois recortes:
> **operação interna** e **destinatário pessoa jurídica**.
>
> Saem daqui: CFOP 6xxx, DIFAL, partilha, FCP interestadual, `refNFe` e as
> finalidades complementar, ajuste e devolução. O que fica é NF-e modelo 55 de
> **venda, saída, finalidade normal, dentro da UF, para destinatário com CNPJ**.
>
> O recorte não some do contrato: cada caso excluído vira **recusa explícita e
> nomeada**, nunca comportamento silencioso. Aceitar um payload interestadual e
> montar o XML errado seria pior do que não aceitá-lo.

## Why

O motor só emite NFC-e (modelo 65). A NF-e é requisito de lançamento, e ela não é
"a mesma coisa com outro número" — muda o que é obrigatório, muda o público e
muda o que o destinatário faz com o documento.

Diferenças que o motor precisa passar a tratar:

- **Destinatário obrigatório e completo**, com endereço e `indIEDest`. Na NFC-e o
  consumidor é opcional, e hoje o adapter crava `indIEDest = NaoContribuinte`.
- **Grupos que a NFC-e não usa**: transporte (`transp`), volumes (`vol`),
  cobrança com duplicatas (`cobr`).
- **`tpNF`**, **`finNFe`**, **`indFinal`** e **`indPres`** deixam de ser
  constantes do adapter e passam a vir do backend.
- **Sem CSC e sem QR Code.** O CSC é da NFC-e. Mandar CSC numa NF-e é erro de
  contrato, e hoje o campo é obrigatório na requisição.
- **DANFE em retrato**, com layout próprio — não é o cupom da NFC-e.

### Três achados da investigação de 13/08/2026

**1. `MontarTotal` zera todo imposto — e isso é um defeito, não uma simplificação.**
O adapter monta `ICMSTot` com `vBC`, `vICMS`, `vPIS` e `vCOFINS` fixos em zero,
somando apenas `vProd`. Passa despercebido porque toda NFC-e emitida até hoje usa
CSOSN 102, que não tem valor a somar — o zero está certo por coincidência. Uma
NF-e com ICMS destacado seria **rejeitada pela SEFAZ**, que confere o total contra
o somatório dos itens. O mesmo vale para uma NFC-e de emitente do Regime Normal:
**o defeito já existe hoje**, só não foi alcançado ainda.

**2. A biblioteca de DANFE atual não gera o modelo 55.** O
`Zeus.Net.NFe.Danfe.QuestPdf` expõe `DanfeNfceDocument` e o layout de eventos —
não há `DanfeNfeDocument`. O DANFE retrato exige outro caminho, e a escolha tem
uma restrição dura: o motor roda em contêiner **Linux**. Ver o `design.md`.

**3. O item de domínio se chama `NfceItem` mas não é mais da NFC-e.** Depois da
etapa 1 ele carrega o quadro tributário e serve aos dois modelos. Uma `Nfe`
composta de `NfceItem` é mentira de nomenclatura em código novo.

## What Changes

- **Rotas novas** `POST /api/nfe/emit`, `/cancel`, `/consulta` e `/danfe`, com DTO
  próprio — os contratos divergem demais para caber num só.
- **`EmitirNfeRequest`** com destinatário obrigatório, `naturezaOperacao`,
  `tpNF`, `finNFe`, `indFinal`, `indPres`, transporte, volumes e cobrança.
- **Reaproveita o quadro tributário do item** criado na etapa 1, sem mudança.
- **`ICMSTot` somado a partir dos itens**, nos dois modelos — corrige o defeito
  latente descrito acima.
- **Recusas nomeadas** para tudo que está fora do recorte: destinatário sem CNPJ,
  UF diferente da do emitente, CFOP fora de `5xxx`, `finNFe` diferente de normal,
  `tpNF` diferente de saída e CSC presente.
- **DANFE modelo 55** em retrato, como documento próprio.
- **`NfceItem` renomeado para `ItemFiscal`**, sem mudança de comportamento.

## Capabilities

### New Capabilities
- `nfe-engine`: emissão, cancelamento, consulta e DANFE da NF-e modelo 55, em
  operação interna para destinatário pessoa jurídica.

### Modified Capabilities
- `fiscal-engine-taxation`: os totais do documento passam a ser somados a partir
  do quadro tributário dos itens, em vez de zerados.

## Impact

- `FiscalService.Application/UseCases/EmitirNfe/` — caso de uso novo, com
  validador próprio; `CancelarNfe/` e `ConsultarNfe/` reaproveitam o serviço de
  eventos, que não distingue modelo.
- `FiscalService.Domain/` — entidade `Nfe`, destinatário obrigatório, transporte,
  volume e cobrança; `NfceItem` vira `ItemFiscal`.
- `FiscalService.Infrastructure/DFe/` — montagem do modelo 55 e correção de
  `MontarTotal`.
- `FiscalService.Infrastructure/Danfe/` — gerador do DANFE retrato.
- **Depende da etapa 1**, arquivada em 13/08/2026 e validada ponta a ponta com
  NFC-e autorizada em homologação.
- **Não depende mais da etapa 2**: o argumento era NF-e interestadual, que saiu do
  recorte.
- Changes irmãs no backend e no frontend.
- Etapa **3** do roteiro fiscal (`gestao_fiscal_backend/ROADMAP_FISCAL.md`).
