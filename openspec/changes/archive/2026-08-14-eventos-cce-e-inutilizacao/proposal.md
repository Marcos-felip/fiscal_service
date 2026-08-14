> **Podada em 14/08/2026**, contra o código real e contra o que a etapa 3
> produziu. Cinco coisas mudaram de lugar — ver "O que a revisão encontrou".

## Why

O motor implementa um evento só: cancelamento (110111). Faltam os outros dois
que toda operação real precisa.

**Carta de Correção Eletrônica (110110).** Depois de autorizada, a nota não pode
ser alterada — mas erro de digitação em campo não essencial acontece o tempo
todo. A CC-e é o instrumento legal para corrigi-lo. Sem ela, o único caminho é
cancelar e reemitir, o que nem sempre cabe no prazo.

**Inutilização.** Quando um número é queimado e nunca vai ser usado, o fisco
exige que a faixa seja formalmente inutilizada. Buraco na numeração é
apontamento.

Isso deixou de ser hipótese em 13/08/2026: a NF-e nº 1 foi **queimada** quando a
criação do documento falhou depois de a numeração já ter sido reservada. Existe
um número perdido na base agora, e não há como regularizá-lo.

## O que a revisão encontrou

**1. Sequência e limite de 20 não são do motor.** A proposta pedia que ele
recusasse repetição, salto e o excesso de correções. O motor é **stateless** —
regra crítica nº 1 do `AGENTS.md` — e não sabe quantas CC-e a nota já teve. Quem
sabe é o backend, que tem `fiscal_document_events`. O motor valida o que é local:
`nSeqEvento` entre 1 e 20. **A regra atravessa para a change irmã.**

**2. "Recusar correção que altere valores" não é implementável, e fingir que é
seria pior.** A CC-e é **texto livre**. Detectar intenção no texto seria
heurística: ou recusa correção legítima ("corrigir o endereço do transportador"),
ou deixa passar a ilegítima com ar de aprovada. A responsabilidade é legal, do
emitente — e o layout já a endereça com o campo `xCondUso`, de texto fixo
obrigatório, que vai no XML. O que cabe fazer é **exibir a condição de uso**,
não simular uma validação que não existe.

**3. Prazo de cancelamento não trava no motor.** O prazo varia por UF — em MG a
NFC-e tem 30 minutos e a NF-e 24 horas, e outras UFs divergem. Quem decide é a
SEFAZ. Travar localmente arriscaria recusar um cancelamento que a UF aceitaria, e
o erro seria invisível: o usuário veria "fora do prazo" sem que ninguém tivesse
perguntado à SEFAZ. A recusa dela já chega legível desde a etapa 1.

**4. Inutilização não é evento.** Tem serviço próprio (`NfeInutilizacao`),
retorno próprio (`retInutNFe`, sem `retEvento`) e não tem chave de acesso nem
protocolo de evento — ela age sobre uma **faixa**, não sobre um documento. Forçar
um "resultado comum aos três" esconderia isso. Ficam dois formatos de retorno,
que é o que existe.

**5. Cancelar já atende os dois modelos.** A etapa 3 introduziu `ModeloDaChave`,
que resolve o modelo pelas posições 21-22 da chave. A tarefa já está cumprida.

## What Changes

- **`POST /api/eventos/carta-correcao`** — evento 110110, com o texto da correção
  validado (15 a 1000 caracteres) e a sequência conferida na faixa legal.
- **`POST /api/eventos/inutilizar`** — inutilização de faixa por série, com
  justificativa, ano, modelo e CNPJ.
- **A condição de uso da CC-e volta no retorno**, para o backend guardá-la junto
  do evento e o frontend mostrá-la antes de confirmar.
- ~~Cancelamento generalizado~~ — feito na etapa 3.
- ~~Sequência conferida no motor~~ — atravessa para o backend.
- ~~Resultado comum aos três eventos~~ — a inutilização não comporta.

## Capabilities

### New Capabilities
- `fiscal-engine-events`: carta de correção e inutilização de numeração.

## Impact

- `FiscalService.Application/UseCases/CartaCorrecao/` e `InutilizarNumeracao/`.
- `FiscalService.Infrastructure/DFe/` — a DFe.NET expõe
  `RecepcaoEventoCartaCorrecao` e `NfeInutilizacao`; o adapter é que não os usa.
  `VersaoRecepcaoEventoCceCancelamento` já está configurado desde o MVP.
- **Sem mudança no contrato de emissão.**
- Changes irmãs no backend (onde moram a sequência e o limite de 20) e no
  frontend.
- Etapa **4** do roteiro fiscal (`gestao_fiscal_backend/ROADMAP_FISCAL.md`).
