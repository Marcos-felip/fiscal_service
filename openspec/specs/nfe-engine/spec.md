# nfe-engine Specification

## Purpose
TBD - created by archiving change emitir-nfe-modelo-55. Update Purpose after archive.
## Requirements
### Requirement: Emissão de NF-e modelo 55 em operação interna
O motor SHALL emitir NF-e modelo 55 de saída, finalidade normal, dentro da UF do
emitente, exigindo destinatário pessoa jurídica com endereço completo e indicador
de inscrição estadual, natureza da operação, indicador de consumidor final e
indicador de presença.

#### Scenario: Emissão de venda a contribuinte da mesma UF
- **WHEN** uma NF-e de saída, finalidade normal, é emitida para destinatário com CNPJ e endereço na mesma UF do emitente
- **THEN** o XML é montado no modelo 55, assinado, transmitido, e o retorno traz chave, protocolo e XML autorizado

#### Scenario: Destinatário ausente
- **WHEN** uma NF-e é solicitada sem destinatário
- **THEN** a emissão é recusada com mensagem indicando que o destinatário é obrigatório no modelo 55

#### Scenario: Destinatário sem indicador de IE
- **WHEN** o destinatário chega sem o indicador de inscrição estadual
- **THEN** a emissão é recusada nomeando o campo

#### Scenario: NF-e sem QR Code
- **WHEN** uma NF-e é autorizada
- **THEN** o retorno não traz QR Code, e nada no fluxo depende dele

### Requirement: O que está fora do recorte é recusado, não aproximado
O motor SHALL recusar, nomeando o motivo, toda requisição de NF-e que esteja fora
do recorte vigente — destinatário pessoa física, operação interestadual, CFOP que
não seja de operação interna, finalidade diferente de normal e nota que não seja
de saída. O motor SHALL NOT montar o XML aproximando um caso que não sabe
representar.

#### Scenario: Destinatário pessoa física
- **WHEN** uma NF-e é solicitada com CPF no destinatário
- **THEN** a emissão é recusada, e a mensagem aponta a NFC-e como o documento da pessoa física

#### Scenario: Destinatário em outra UF
- **WHEN** a UF do destinatário difere da UF do emitente
- **THEN** a emissão é recusada informando que a operação interestadual está fora do escopo atual

#### Scenario: CFOP de operação interestadual
- **WHEN** um item traz CFOP fora da faixa de operação interna
- **THEN** a emissão é recusada apontando o item e o CFOP

#### Scenario: Finalidade não suportada
- **WHEN** a finalidade informada é complementar, ajuste ou devolução
- **THEN** a emissão é recusada nomeando a finalidade recebida

### Requirement: Indicador de IE coerente com a inscrição estadual
O motor SHALL exigir inscrição estadual quando o destinatário for declarado
contribuinte, e SHALL recusar inscrição estadual quando ele for declarado isento
ou não contribuinte.

#### Scenario: Contribuinte sem inscrição estadual
- **WHEN** o destinatário é declarado contribuinte e não traz IE
- **THEN** a emissão é recusada

#### Scenario: Não contribuinte com inscrição estadual
- **WHEN** o destinatário é declarado não contribuinte e traz IE
- **THEN** a emissão é recusada, porque a declaração e o dado se contradizem

#### Scenario: Isento de inscrição estadual
- **WHEN** o destinatário é declarado isento de IE e não traz IE
- **THEN** a emissão prossegue

### Requirement: Grupos exclusivos da NF-e
O motor SHALL montar os grupos de transporte, volumes e cobrança com duplicatas
quando informados, e SHALL omiti-los quando não informados.

#### Scenario: Nota com transporte e volumes
- **WHEN** a requisição traz transportadora, placa, peso bruto e líquido
- **THEN** o XML contém os grupos de transporte e volumes preenchidos

#### Scenario: Nota a prazo com duplicatas
- **WHEN** a requisição traz cobrança com parcelas
- **THEN** o XML contém o grupo de cobrança com uma duplicata por parcela

#### Scenario: Nota sem transporte informado
- **WHEN** a requisição não traz transporte
- **THEN** o XML declara ausência de frete, sem grupo de volumes

### Requirement: CSC não pertence à NF-e
O motor SHALL recusar requisição de NF-e que informe CSC ou ID do CSC, em vez de
ignorá-los.

#### Scenario: CSC enviado por engano
- **WHEN** uma requisição de NF-e traz CSC
- **THEN** a emissão é recusada com mensagem explicando que o CSC é exclusivo da NFC-e

### Requirement: DANFE do modelo 55
O motor SHALL gerar o DANFE da NF-e como documento próprio, distinto do cupom da
NFC-e, e SHALL declarar o tipo de conteúdo gerado para que o chamador saiba
armazená-lo e servi-lo.

#### Scenario: DANFE de NF-e autorizada
- **WHEN** o DANFE de uma NF-e autorizada é solicitado
- **THEN** o motor devolve o documento com os dados do emitente, do destinatário, dos itens, dos impostos e das duplicatas, junto do tipo de conteúdo

### Requirement: Cancelamento e consulta do modelo 55
O motor SHALL cancelar e consultar NF-e pelos mesmos eventos já usados na NFC-e,
resolvendo UF e CNPJ a partir da própria chave de acesso.

#### Scenario: Cancelamento de NF-e
- **WHEN** o cancelamento de uma NF-e autorizada é solicitado com justificativa válida
- **THEN** o evento é transmitido e o retorno indica homologação ou rejeição

#### Scenario: Consulta de NF-e
- **WHEN** a situação de uma NF-e é consultada pela chave
- **THEN** o motor devolve a situação atual informada pela SEFAZ

