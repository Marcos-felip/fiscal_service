## Why

O motor implementa um evento só: cancelamento (110111). Faltam os outros dois
que toda operação real precisa.

**Carta de Correção Eletrônica (110110).** Depois de autorizada, a nota não pode
ser alterada — mas erros de digitação em campos não essenciais acontecem o tempo
todo. A CC-e é o instrumento legal para corrigi-los, e tem regras próprias que o
código precisa impor: até 20 por nota, sequência incremental, e **não pode
alterar valores, datas, remetente nem destinatário**. Emitir CC-e para corrigir
valor é infração, não erro de sistema.

**Inutilização.** Quando um número de nota é queimado e nunca vai ser usado — a
emissão falhou de forma definitiva, houve salto na sequência — o fisco exige que
a faixa seja formalmente inutilizada. Sem isso fica um buraco na numeração, e
buraco na numeração é apontamento.

Hoje as duas situações não têm resposta no sistema.

## What Changes

- **`POST /api/eventos/carta-correcao`** — evento 110110, com validação do texto
  da correção (15 a 1000 caracteres) e da sequência.
- **`POST /api/eventos/inutilizar`** — inutilização de faixa de numeração, por
  série, com justificativa.
- **Cancelamento generalizado para os dois modelos.** O `Cancelar` atual assume
  NFC-e; passa a servir NF-e também, respeitando os prazos de cada modelo.
- **Sequência da CC-e conferida no motor**: `nSeqEvento` incremental, recusando
  repetição e salto.
- **Retorno padronizado entre os eventos**, para o backend tratar os três pelo
  mesmo caminho.

## Capabilities

### New Capabilities
- `fiscal-engine-events`: eventos fiscais além do cancelamento — carta de
  correção e inutilização de numeração.

### Modified Capabilities
- `nfe-engine`: o cancelamento passa a atender explicitamente os dois modelos.

## Impact

- `FiscalService.Application/UseCases/` — casos de uso novos para CC-e e
  inutilização.
- `FiscalService.Infrastructure/DFe/DFeNetAdapter.cs` — a DFe.NET já expõe os
  serviços; o adapter é que não os usa. `VersaoRecepcaoEventoCceCancelamento` já
  está configurado.
- **Regras que não podem ficar implícitas**: o limite de 20 CC-e e a proibição de
  alterar valores precisam de validação explícita e mensagem clara. Confiar na
  SEFAZ para rejeitar é deixar o usuário descobrir tarde.
- **Revisar antes de implementar** — ver o aviso no roteiro fiscal.
- Changes irmãs no backend e no frontend.
- Etapa **4** do roteiro fiscal (`gestao_fiscal_backend/ROADMAP_FISCAL.md`).
