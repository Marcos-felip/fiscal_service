## ADDED Requirements

### Requirement: Carta de Correção Eletrônica
O motor SHALL transmitir o evento de carta de correção (110110) para documento
autorizado, com o texto da correção entre 15 e 1000 caracteres e sequência
incremental por nota.

#### Scenario: Primeira correção
- **WHEN** uma CC-e é solicitada para uma nota autorizada, com texto válido
- **THEN** o evento é transmitido com sequência 1 e o retorno traz a situação informada pela SEFAZ

#### Scenario: Correção subsequente
- **WHEN** uma segunda CC-e é solicitada para a mesma nota
- **THEN** o evento é transmitido com sequência 2

#### Scenario: Texto curto demais
- **WHEN** o texto da correção tem menos de 15 caracteres
- **THEN** a requisição é recusada antes da transmissão, com mensagem indicando o mínimo

#### Scenario: Limite de correções
- **WHEN** já existem 20 cartas de correção para a nota
- **THEN** a requisição é recusada com mensagem explicando o limite legal

#### Scenario: Sequência repetida ou salteada
- **WHEN** a sequência informada já foi usada ou pula um número
- **THEN** a requisição é recusada antes da transmissão

### Requirement: A CC-e não corrige o que a lei proíbe
O motor SHALL recusar carta de correção que pretenda alterar valores, datas,
emitente ou destinatário, com mensagem explicando que esses campos exigem
cancelamento e nova emissão.

#### Scenario: Tentativa de corrigir valor
- **WHEN** a correção indica mudança de valor da nota
- **THEN** a requisição é recusada com orientação de cancelar e reemitir

### Requirement: Inutilização de faixa de numeração
O motor SHALL transmitir a inutilização de uma faixa de numeração de uma série,
com justificativa, para o modelo e ambiente informados.

#### Scenario: Inutilizar faixa
- **WHEN** a inutilização de uma faixa é solicitada com justificativa válida
- **THEN** o evento é transmitido e o retorno traz o protocolo

#### Scenario: Faixa invertida
- **WHEN** o número inicial é maior que o final
- **THEN** a requisição é recusada antes da transmissão

#### Scenario: Justificativa curta demais
- **WHEN** a justificativa tem menos que o mínimo exigido
- **THEN** a requisição é recusada com mensagem indicando o mínimo

### Requirement: Cancelamento para os dois modelos
O cancelamento SHALL atender NFC-e e NF-e, respeitando as regras de prazo de cada
modelo e mantendo o retorno padronizado.

#### Scenario: Cancelamento de NF-e
- **WHEN** o cancelamento de uma NF-e autorizada é solicitado
- **THEN** o evento é transmitido no mesmo formato usado para NFC-e

#### Scenario: Cancelamento homologado fora do prazo
- **WHEN** a SEFAZ responde que o cancelamento foi homologado fora do prazo
- **THEN** o motor trata como sucesso, como já faz hoje

### Requirement: Retorno padronizado entre eventos
Os três eventos SHALL devolver a mesma estrutura de resultado — sucesso, código,
motivo, protocolo e XML — para que o consumidor os trate por um caminho só.

#### Scenario: Consumo uniforme
- **WHEN** o backend processa o retorno de cancelamento, carta de correção ou inutilização
- **THEN** encontra os mesmos campos, sem tratamento específico por tipo de evento
