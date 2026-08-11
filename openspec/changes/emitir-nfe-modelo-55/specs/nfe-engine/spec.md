## ADDED Requirements

### Requirement: Emissão de NF-e modelo 55
O motor SHALL emitir NF-e modelo 55, exigindo destinatário completo com endereço
e indicador de inscrição estadual, natureza da operação, tipo de operação
(entrada ou saída) e finalidade da nota.

#### Scenario: Emissão de venda a contribuinte
- **WHEN** uma NF-e de saída, finalidade normal, é emitida para destinatário contribuinte em outra UF
- **THEN** o XML é montado no modelo 55, assinado, transmitido, e o retorno traz chave, protocolo e XML autorizado

#### Scenario: Destinatário ausente
- **WHEN** uma NF-e é solicitada sem destinatário
- **THEN** a emissão é recusada com mensagem indicando que o destinatário é obrigatório no modelo 55

#### Scenario: Destinatário sem indicador de IE
- **WHEN** o destinatário chega sem o indicador de inscrição estadual
- **THEN** a emissão é recusada nomeando o campo

### Requirement: CFOP interestadual aceito
O motor SHALL aceitar CFOP de operação interestadual na NF-e, sem a restrição a
operações internas aplicada à NFC-e.

#### Scenario: Operação interestadual
- **WHEN** um item da NF-e traz CFOP de operação interestadual
- **THEN** a emissão prossegue normalmente

#### Scenario: CFOP incoerente com as UFs
- **WHEN** o CFOP indica operação interna mas as UFs de emitente e destinatário diferem
- **THEN** a emissão é recusada apontando a incoerência

### Requirement: Grupos exclusivos da NF-e
O motor SHALL montar os grupos de transporte, volumes, cobrança com duplicatas e
documento referenciado quando informados.

#### Scenario: Nota com transporte e volumes
- **WHEN** a requisição traz transportadora, placa, peso bruto e líquido
- **THEN** o XML contém os grupos de transporte e volumes preenchidos

#### Scenario: Nota a prazo com duplicatas
- **WHEN** a requisição traz cobrança com parcelas
- **THEN** o XML contém o grupo de cobrança com uma duplicata por parcela

#### Scenario: Documento referenciado
- **WHEN** a requisição informa a chave de uma nota referenciada
- **THEN** o XML registra a referência no grupo correspondente

### Requirement: CSC não pertence à NF-e
O motor SHALL recusar requisição de NF-e que informe CSC ou ID do CSC, em vez de
ignorá-los.

#### Scenario: CSC enviado por engano
- **WHEN** uma requisição de NF-e traz CSC
- **THEN** a emissão é recusada com mensagem explicando que o CSC é exclusivo da NFC-e

#### Scenario: NF-e sem QR Code
- **WHEN** uma NF-e é autorizada
- **THEN** o retorno não traz QR Code, e nada no fluxo depende dele

### Requirement: DANFE do modelo 55
O motor SHALL gerar o DANFE da NF-e em formato retrato, distinto do cupom da
NFC-e.

#### Scenario: DANFE de NF-e autorizada
- **WHEN** o DANFE de uma NF-e autorizada é solicitado
- **THEN** o motor devolve o PDF em retrato, com os dados do destinatário, dos itens, dos impostos e das duplicatas

### Requirement: Cancelamento e consulta do modelo 55
O motor SHALL cancelar e consultar NF-e pelos mesmos eventos já usados na NFC-e,
respeitando as regras de prazo do modelo 55.

#### Scenario: Cancelamento de NF-e
- **WHEN** o cancelamento de uma NF-e autorizada é solicitado com justificativa válida
- **THEN** o evento é transmitido e o retorno indica homologação ou rejeição

#### Scenario: Consulta de NF-e
- **WHEN** a situação de uma NF-e é consultada pela chave
- **THEN** o motor devolve a situação atual informada pela SEFAZ
