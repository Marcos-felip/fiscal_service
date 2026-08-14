> **Podada em 14/08/2026** contra o código real. O que saiu daqui e por quê está
> em "O que a revisão encontrou", no `proposal.md`.

## 1. Pré-requisitos

- [x] 1.1 Etapa 3 (`emitir-nfe-modelo-55`) aplicada e arquivada em 14/08/2026
- [x] 1.2 Revisar esta proposta contra o que a etapa 3 produziu — feito nesta poda, com cinco achados

## 2. Carta de Correção

- [x] 2.1 Caso de uso, DTO e validador da CC-e (evento 110110)
- [x] 2.2 Texto da correção entre 15 e 1000 caracteres
- [x] 2.3 `nSeqEvento` entre 1 e 20 — a **faixa legal**, que é o que o motor sabe conferir
- [x] 2.4 ~~Recusar repetição e salto de sequência~~ · ~~limite de 20 por nota~~ — **atravessa para o backend:** exige o histórico da nota, e o motor não persiste nada
- [x] 2.5 ~~Recusar correção que altere valores~~ — **não implementável:** a CC-e é texto livre, e detectar intenção seria heurística. Vira `xCondUso` no XML e aviso na tela
- [x] 2.6 `RecepcaoEventoCartaCorrecao` no adapter
- [x] 2.7 Devolver a condição de uso, para o backend guardá-la e o frontend mostrá-la antes de confirmar

## 3. Inutilização

- [x] 3.1 Caso de uso, DTO e validador: CNPJ, ano, modelo, série, faixa e justificativa
- [x] 3.2 Faixa coerente (inicial ≤ final) e justificativa de 15 a 255 caracteres
- [x] 3.3 `NfeInutilizacao` no adapter, com retorno próprio — ela age sobre uma faixa, não sobre um documento

## 4. Cancelamento

- [x] 4.1 ~~Generalizar `Cancelar` para NF-e~~ — **feito na etapa 3:** `ModeloDaChave` resolve o modelo pelas posições 21-22 da chave
- [x] 4.2 ~~Prazos por modelo no domínio~~ — **descartado:** o prazo varia por UF e quem decide é a SEFAZ. Travar localmente recusaria cancelamento que a UF aceitaria

## 5. Rotas

- [x] 5.1 `POST /api/eventos/carta-correcao`
- [x] 5.2 `POST /api/eventos/inutilizar`
- [x] 5.3 ~~Resultado comum aos três eventos~~ — a inutilização não tem chave nem protocolo de evento; um formato só esconderia isso

## 6. Testes

- [x] 6.1 CC-e: texto curto, texto longo e sequência fora da faixa recusados
- [x] 6.2 Inutilização: faixa invertida e justificativa curta recusadas
- [x] 6.3 A condição de uso volta no retorno, **inclusive na recusa** — quem confirma precisa lê-la antes. O `xCondUso` do XML é preenchido pela própria DFe.NET, e testá-lo exigiria certificado e rede: fica coberto na emissão real
- [x] 6.4 **Regressão:** cancelamento de NFC-e e de NF-e inalterados
- [x] 6.5 `dotnet test` verde

## 7. Documentação

- [x] 7.1 `AGENTS.md`: os três eventos, e o que é do motor e o que é do backend
- [x] 7.2 `docs/`: contrato dos dois endpoints novos, para a change irmã do backend
