> **Reveja esta proposta antes de começar** — escrita antes da etapa 3.

## 1. Pré-requisito

- [ ] 1.1 Etapa 3 (`emitir-nfe-modelo-55`) aplicada
- [ ] 1.2 Revisar esta proposta contra o que a etapa 3 produziu

## 2. Carta de Correção

- [ ] 2.1 Caso de uso e DTO da CC-e (evento 110110)
- [ ] 2.2 Validar texto entre 15 e 1000 caracteres
- [ ] 2.3 Validar sequência incremental: recusar repetição e salto
- [ ] 2.4 Recusar até 20 correções por nota, com mensagem citando o limite legal
- [ ] 2.5 Recusar correção que pretenda alterar valores, datas, emitente ou destinatário
- [ ] 2.6 `RecepcaoEvento` de CC-e no adapter, aproveitando `VersaoRecepcaoEventoCceCancelamento` já configurado

## 3. Inutilização

- [ ] 3.1 Caso de uso e DTO: série, faixa inicial e final, justificativa, modelo, ambiente
- [ ] 3.2 Validar faixa (inicial ≤ final) e justificativa mínima
- [ ] 3.3 Chamada de inutilização no adapter

## 4. Cancelamento para os dois modelos

- [ ] 4.1 Generalizar `Cancelar` para NF-e, mantendo `cStat` 135 e 155 como sucesso
- [ ] 4.2 Prazos por modelo explicitados no domínio, não no adapter

## 5. Padronização

- [ ] 5.1 Resultado comum aos três eventos: sucesso, código, motivo, protocolo, XML
- [ ] 5.2 Rotas sob `/api/eventos/`

## 6. Testes

- [ ] 6.1 CC-e: texto curto, sequência repetida, sequência salteada, limite de 20
- [ ] 6.2 CC-e que tenta corrigir valor é recusada
- [ ] 6.3 Inutilização com faixa invertida e justificativa curta recusadas
- [ ] 6.4 Cancelamento de NF-e no mesmo formato do de NFC-e
- [ ] 6.5 **Regressão:** cancelamento de NFC-e inalterado
- [ ] 6.6 `dotnet test` verde

## 7. Documentação

- [ ] 7.1 `AGENTS.md` do motor: os três eventos e suas regras
- [ ] 7.2 Publicar o contrato para a change irmã do backend
