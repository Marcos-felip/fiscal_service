## Why

O motor decide imposto. Não deveria.

`MontarImposto` (`DFeNetAdapter.cs`) recebe do backend apenas `origem` e
`csosn`, e completa o resto com regra fixa no código:

```csharp
PIS    = new PIS    { TipoPIS    = new PISNT    { CST = CSTPIS.pis07 } },
COFINS = new COFINS { TipoCOFINS = new COFINSNT { CST = CSTCOFINS.cofins07 } }
```

Toda nota sai com PIS e COFINS em CST 07 — "operação isenta da contribuição" —
independentemente do produto. Empresa do Simples não é isenta: paga via DAS.
E produto monofásico (bebida fria, por exemplo) tem CST próprio de revenda.

Isso não é rejeitado pela SEFAZ, que valida estrutura e não a coerência do CST
com a natureza do produto. O erro aparece depois, na escrituração do contador —
que recebe só o XML e não tem como perceber.

O comentário do próprio método reconhece a restrição:

> *"O payload recebido do backend traz apenas origem e CSOSN/CST — nao traz base
> de calculo nem aliquotas. Por isso so sao aceitas as situacoes tributarias que
> se resolvem sem valores; as demais falham explicitamente em vez de emitir
> imposto zerado."*

Falhar alto foi a decisão certa dentro da restrição. Esta change remove a
restrição: o item passa a carregar o quadro tributário completo, e o motor deixa
de decidir para passar a **traduzir**.

Sem isso, nada do roteiro fiscal anda: devolução espelha impostos que a nota
original não guardou, Regime Normal não emite (só CST 40/41/50 são aceitos), e
interestadual não tem como destacar ICMS.

## What Changes

- **`ItemDto` ganha o bloco tributário**: `icms`, `ipi`, `pis` e `cofins`, cada
  um com situação tributária, base de cálculo, alíquota e valores.
- **`MontarImposto` vira tradutor**: recebe o quadro e monta o grupo XML
  correspondente. Deixa de escolher CST, deixa de zerar valor.
- **PIS e COFINS deixam de ser fixos** — vêm do payload, com o CST que o emitente
  determinou.
- **Amplia as situações tributárias aceitas.** O que hoje é recusado por falta de
  valores passa a ser possível: CSOSN `101`, `201`, `202`, `203`, `900` e CST de
  ICMS `00`, `10`, `20`, `51`, `60`, `70`, `90`.
- **Grupos de valor que hoje não existem**: `ICMSST` (substituição), FCP,
  partilha interestadual e crédito do Simples (`pCredSN`/`vCredICMSSN`).
- **Sem fallback.** O bloco tributário é obrigatório, e `origem`/`csosn` saem do nível
  do item. O fallback temporário chegou a ser implementado, mas o backend passou a
  publicar o bloco na mesma janela — não havia ninguém no caminho antigo para proteger.
- **A validação de coerência sai do motor.** Recusar CST incompatível com o
  regime deixa de ser regra escondida no C# e vira validação declarada sobre o
  quadro recebido: se veio base sem alíquota, se veio valor que não fecha com
  base × alíquota, se o CST exige campo que não veio.

## Capabilities

### New Capabilities
- `fiscal-engine-taxation`: contrato tributário por item e tradução para os
  grupos de imposto do XML da NF-e/NFC-e.

### Modified Capabilities
<!-- `openspec/specs/` deste repositório está vazio: esta é a primeira change do
motor a ser especificada. As capabilities anteriores nunca foram registradas. -->

## Impact

- `FiscalService.Application/DTOs/`: `ItemDto` ganha o bloco tributário; DTOs
  novos para ICMS, IPI, PIS e COFINS.
- `FiscalService.Domain/`: value objects da situação tributária, com as regras de
  quais campos cada CST exige.
- `FiscalService.Infrastructure/DFe/DFeNetAdapter.cs`: `MontarImposto`,
  `MontarIcmsSimplesNacional` e `MontarIcmsRegimeNormal` reescritos como
  tradução; `ValidacoesFiscais` deixa de ter lista curta de CST suportado.
- `FiscalService.Application/UseCases/EmitirNfce/EmitirNfceValidator.cs`: valida
  a coerência do quadro em vez de restringir o conjunto de códigos.
- **Mudança de contrato**: é a primeira quebra desde a Fase A, e é uma quebra
  assumida — motor e backend sobem juntos.
- **Este repositório não tem nenhum teste** (`tests/` só tem andaime). Esta
  change cria os primeiros: são regras fiscais, e regra fiscal sem teste é
  rejeição em produção.
- Changes irmãs: `gestao_fiscal_backend` e `gestao_fiscal_frontend`, ambas
  `contrato-tributario-do-item`.
- Etapa **1** do roteiro fiscal (`gestao_fiscal_backend/ROADMAP_FISCAL.md`).
