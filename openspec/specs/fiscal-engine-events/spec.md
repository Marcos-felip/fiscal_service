# fiscal-engine-events Specification

## Purpose
Os eventos que o motor transmite depois da emissão e que não são o cancelamento:
a carta de correção (110110) e a inutilização de faixa de numeração — esta
última, que sequer é evento de documento, porque age sobre números que nunca
viraram nota.

## Requirements
### Requirement: Carta de Correção Eletrônica
O motor SHALL transmitir o evento de carta de correção (110110) para documento
autorizado, recusando antes da transmissão o texto fora de 15 a 1000 caracteres e
a sequência fora da faixa legal de 1 a 20.

O motor **não** confere se a sequência repete ou salta, nem quantas correções a
nota já teve: isso exige o histórico do documento, e o motor não persiste nada.
Quem impõe essas duas regras é o chamador, que tem os eventos gravados.

#### Scenario: Correção transmitida
- **WHEN** uma CC-e é solicitada para uma nota autorizada, com texto e sequência válidos
- **THEN** o evento é transmitido e o retorno traz a situação informada pela SEFAZ

#### Scenario: Texto curto demais
- **WHEN** o texto da correção tem menos de 15 caracteres
- **THEN** a requisição é recusada antes da transmissão, com mensagem indicando o mínimo

#### Scenario: Texto longo demais
- **WHEN** o texto da correção passa de 1000 caracteres
- **THEN** a requisição é recusada antes da transmissão

#### Scenario: Sequência fora da faixa legal
- **WHEN** a sequência informada é menor que 1 ou maior que 20
- **THEN** a requisição é recusada citando o limite de 20 correções por nota

### Requirement: A condição de uso da CC-e viaja com o evento
O motor SHALL incluir no XML o texto legal de condição de uso da carta de
correção, e SHALL devolvê-lo no retorno para que o chamador possa apresentá-lo
antes da confirmação.

O motor SHALL NOT tentar deduzir, do texto livre da correção, se ela altera
valores, datas ou as partes. A CC-e é texto livre; inferir intenção seria
heurística, que ou recusa correção legítima ou aprova a ilegítima. A restrição é
legal e recai sobre o emitente — o instrumento que o layout oferece é justamente
declarar a condição de uso, não filtrar o texto.

#### Scenario: Condição de uso no XML
- **WHEN** uma CC-e é montada
- **THEN** o XML contém o texto legal de condição de uso

#### Scenario: Condição de uso devolvida
- **WHEN** o motor responde a uma solicitação de CC-e
- **THEN** o retorno traz a condição de uso, para exibição a quem confirma

### Requirement: Inutilização de faixa de numeração
O motor SHALL transmitir a inutilização de uma faixa de numeração de uma série,
recusando antes da transmissão a faixa invertida e a justificativa fora de 15 a
255 caracteres.

A inutilização age sobre uma **faixa**, não sobre um documento: ela não tem chave
de acesso nem protocolo de evento, e por isso seu retorno é próprio. Pela mesma
razão a UF viaja no pedido — nas outras rotas ela é deduzida da chave.

#### Scenario: Inutilizar faixa
- **WHEN** a inutilização de uma faixa é solicitada com justificativa válida
- **THEN** o pedido é transmitido e o retorno traz a situação e o protocolo da SEFAZ

#### Scenario: Faixa invertida
- **WHEN** o número inicial é maior que o final
- **THEN** a requisição é recusada antes da transmissão

#### Scenario: Justificativa curta demais
- **WHEN** a justificativa tem menos de 15 caracteres
- **THEN** a requisição é recusada com mensagem indicando o mínimo

#### Scenario: Número único
- **WHEN** o número inicial é igual ao final
- **THEN** a faixa é aceita, porque inutilizar um número só é o caso comum

### Requirement: O ano da inutilização vai com dois dígitos
O motor SHALL converter o ano recebido para dois dígitos ao montar o pedido de
inutilização.

O identificador do pedido tem tamanho fixo (`ID + cUF + ano + CNPJ + modelo +
série + faixa`). Enviar o ano com quatro dígitos estica o identificador e a SEFAZ
recusa com `215 — Falha no esquema XML`, sem indicar o campo. O contrato continua
falando em ano cheio; a conversão é do adapter.

#### Scenario: Ano convertido
- **WHEN** o chamador informa o ano 2026
- **THEN** o pedido transmitido leva 26

### Requirement: Espera pela SEFAZ compatível com o serviço
O motor SHALL usar um tempo de espera pelos serviços da SEFAZ maior que o padrão
da biblioteca.

Cinco segundos são insuficientes: o pedido chega e é homologado, mas a resposta
não volta a tempo. Para quem chama isso é indistinguível de falha — e o ato já
está praticado do outro lado, sem protocolo registrado deste.

#### Scenario: Serviço lento
- **WHEN** a SEFAZ demora mais que o padrão da biblioteca para responder
- **THEN** o motor continua aguardando até o limite configurado, em vez de tratar como falha de comunicação
