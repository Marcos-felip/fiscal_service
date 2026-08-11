> **Reveja esta proposta antes de começar.** Ela foi escrita antes de a etapa 1
> existir. Rode `openspec-update-change` primeiro — o contrato do item vai ter
> mudado detalhes daqui.

## 1. Pré-requisito

- [ ] 1.1 Etapa 1 (`contrato-tributario-do-item`) aplicada, com o bloco tributário no item
- [ ] 1.2 Revisar esta proposta contra o contrato real que a etapa 1 produziu

## 2. Contrato

- [ ] 2.1 `EmitirNfeRequest` — destinatário obrigatório com endereço e `indIEDest`, `naturezaOperacao`, `tpNF`, `finNFe`
- [ ] 2.2 `refNFe` para documento referenciado
- [ ] 2.3 Grupos de transporte, volumes e cobrança com duplicatas
- [ ] 2.4 Reaproveitar o bloco tributário do item, estendido com DIFAL, partilha e FCP
- [ ] 2.5 Recusar CSC e ID do CSC na requisição de NF-e

## 3. Domínio

- [ ] 3.1 Value objects: destinatário completo, transporte, volume, cobrança
- [ ] 3.2 Validação de coerência entre CFOP e UFs de emitente e destinatário
- [ ] 3.3 Regras de obrigatoriedade por `finNFe`

## 4. Montagem do XML

- [ ] 4.1 `MontarNfe` no adapter, modelo 55
- [ ] 4.2 Grupos `dest`, `transp`, `vol`, `cobr`, `NFref`
- [ ] 4.3 DIFAL, partilha e FCP no grupo de ICMS
- [ ] 4.4 Sem QR Code e sem `infNFeSupl`

## 5. Rotas e operações

- [ ] 5.1 `POST /api/nfe/emit`
- [ ] 5.2 `POST /api/nfe/cancel` e `/consultar`, reaproveitando o serviço de eventos
- [ ] 5.3 `POST /api/nfe/danfe` — DANFE em retrato via QuestPDF

## 6. Testes

- [ ] 6.1 Emissão de saída a contribuinte interestadual, conferindo o XML gerado
- [ ] 6.2 Destinatário ausente ou sem `indIEDest` recusado
- [ ] 6.3 CFOP incoerente com as UFs recusado
- [ ] 6.4 CSC em requisição de NF-e recusado
- [ ] 6.5 Transporte, volumes e cobrança presentes no XML quando informados
- [ ] 6.6 **Regressão:** a emissão de NFC-e continua igual
- [ ] 6.7 `dotnet test` verde

## 7. Documentação

- [ ] 7.1 `AGENTS.md` do motor: contrato da NF-e e o que difere da NFC-e
- [ ] 7.2 Publicar o contrato para a change irmã do backend
