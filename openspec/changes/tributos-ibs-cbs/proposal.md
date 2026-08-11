> ⚠️ **Esta proposta precisa ser revalidada antes de qualquer implementação.**
> Ela foi escrita em 11/08/2026 contra o entendimento da reforma tributária
> naquele momento. Os grupos de IBS/CBS e a tabela de `cClassTrib` vêm sendo
> revisados entre versões do Informe Técnico. **Confira a NT vigente antes de
> escrever a primeira linha** — o que estiver aqui pode estar desatualizado.

## Why

A Reforma Tributária do Consumo alterou o leiaute da NF-e/NFC-e. A CBS entra
valendo em 2027, e o IBS na sequência. Uma plataforma que lança agora e pretende
operar além disso vai precisar dos grupos novos.

A boa notícia é concreta: a `Zeus.Net.NFe.NFCe` na versão **2026.7.16** já traz
os grupos da reforma. O trabalho é de adapter e contrato, **não** de troca de
biblioteca.

Evidência de que hoje não é bloqueante: a nota 7 foi autorizada pela SEFAZ-MG em
10/08/2026 sem nenhum grupo de IBS/CBS.

## What Changes

- **Bloco de IBS/CBS por item**, ao lado dos de ICMS, IPI, PIS e COFINS já
  criados na etapa 1: CST próprio, classificação tributária (`cClassTrib`),
  alíquotas de IBS estadual e municipal, alíquota de CBS, reduções,
  diferimentos, créditos presumidos e valores.
- **Totais de IBS e CBS** no grupo de totais.
- **Imposto Seletivo**, quando aplicável ao produto.
- **A classificação tributária não vira enum fixo no código.** A tabela oficial é
  atualizada com frequência; ela precisa ser dado, resolvido pelo backend e
  transportado pelo contrato — repetir aqui o erro do CST fixo seria não ter
  aprendido nada com a etapa 1.
- **Convivência com o regime atual** durante a transição: nota que não exige os
  grupos continua saindo sem eles.

## Capabilities

### New Capabilities
- `fiscal-engine-ibs-cbs`: grupos de IBS, CBS e Imposto Seletivo no XML.

### Modified Capabilities
- `fiscal-engine-taxation`: o quadro tributário do item passa a comportar os
  tributos da reforma.

## Impact

- `FiscalService.Application/DTOs/` — bloco novo no item e nos totais.
- `FiscalService.Infrastructure/DFe/` — tradução para os grupos da DFe.NET.
- **Verificar a versão da NT que a biblioteca implementa** antes de começar; pode
  ser necessário atualizar o pacote.
- **Depende da etapa 1**: o desenho do bloco tributário precisa ter deixado
  espaço para estes tributos.
- Change irmã no backend.
- Etapa **6** do roteiro fiscal (`gestao_fiscal_backend/ROADMAP_FISCAL.md`).
