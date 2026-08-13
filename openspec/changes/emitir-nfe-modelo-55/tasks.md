> **Podada em 13/08/2026** contra o código real, com as decisões em `design.md`.
> Recorte: operação **interna**, destinatário **pessoa jurídica**, saída,
> finalidade normal. O que está fora vira recusa nomeada, não silêncio.

## 1. Pré-requisitos

- [x] 1.1 Etapa 1 (`contrato-tributario-do-item`) aplicada e **arquivada em 13/08/2026**, com o quadro tributário no item
- [x] 1.2 Revisar esta proposta contra o contrato real que a etapa 1 produziu — feito nesta poda, com três achados registrados no `proposal.md`
- [x] 1.3 Escolher o caminho do DANFE do modelo 55 — medido em contêiner Linux, ver D6

## 2. Renomeação e correção anteriores à NF-e

> Duas coisas que a NF-e expõe mas que não são dela. Ficam antes, em commit
> próprio, para não se misturarem com a funcionalidade nova.

- [x] 2.1 `NfceItem` → `ItemFiscal`, sem mudança de comportamento (D1)
- [x] 2.2 `MontarTotal` soma `vBC`, `vICMS`, `vST`, `vFCP`, `vPIS`, `vCOFINS` e `vIPI` a partir do quadro tributário dos itens (D5)
- [x] 2.3 Teste do total somado: item com ICMS destacado leva `ICMSTot` coerente
- [x] 2.4 **Regressão:** NFC-e com CSOSN 102 continua com todos os totais em zero
- [x] 2.5 **Achado ao rodar em Linux:** a mensagem de recusa mudava de formato com o locale do processo — "10,00" no Windows, "10.00" no contêiner. Passou por `FormatoFiscal.Valor`, sempre pt-BR

## 3. Contrato

- [x] 3.1 `EmitirNfeRequest`: emitente, destinatário obrigatório, itens, pagamentos, certificado, série, número, ambiente
- [x] 3.2 `DestinatarioNfeDto` com endereço completo, `indIEDest` e IE opcional
- [x] 3.3 `naturezaOperacao`, `tpNF`, `finNFe`, `indFinal`, `indPres` (D4)
- [x] 3.4 Grupos de transporte, volumes e cobrança com duplicatas, todos opcionais
- [x] 3.5 `EmitirNfeResponse` com `danfeContentType`, sem QR Code (D6)
- [x] 3.6 Sem CSC no contrato — informar é recusa, não campo ignorado

## 4. Domínio

- [x] 4.1 Entidade `Nfe`, reusando `ItemFiscal` e `Pagamento`
- [x] 4.2 `DestinatarioNfe` com endereço e `indIEDest` obrigatórios por construção (D2)
- [x] 4.3 Value objects de transporte, volume e cobrança
- [x] 4.4 `IndicadorIeDestinatario`, `TipoOperacao`, `FinalidadeNfe`, `PresencaComprador` como enums de domínio

## 5. Validação — o recorte, nomeado

- [x] 5.1 Destinatário com CPF recusado, apontando a NFC-e como o caminho da pessoa física
- [x] 5.2 UF do destinatário diferente da do emitente recusada
- [x] 5.3 CFOP fora de `5xxx` recusado
- [x] 5.4 `finNFe` diferente de normal e `tpNF` diferente de saída recusados
- [x] 5.5 Contribuinte sem IE recusado; isento e não contribuinte recusados **com** IE
- [x] 5.6 CSC presente recusado

## 6. Montagem do XML

- [x] 6.1 `MontarNfe` do modelo 55: `mod`, `tpImp` retrato, `idDest` interna, sem `infNFeSupl`
- [x] 6.2 Grupo `dest` completo, com `indIEDest` e IE
- [x] 6.3 Grupos `transp`, `vol` e `cobr` quando informados
- [x] 6.4 `ModeloDocumento.NFe` na configuração do serviço e na chave de acesso
- [x] 6.5 Texto obrigatório de homologação aplicado ao destinatário e ao primeiro item

## 7. Rotas

- [x] 7.1 `POST /api/nfe/emit`
- [x] 7.2 `POST /api/nfe/cancel` e `/api/nfe/consulta`, delegando aos handlers existentes (D7)
- [x] 7.3 `POST /api/nfe/danfe`, HTML a partir do XML autorizado

## 8. DANFE

- [x] 8.1 Pacote `Zeus.Net.NFe.Danfe.Html` referenciado
- [x] 8.2 Gerador do DANFE 55, atrás de uma porta própria — o gerador da NFC-e continua PDF
- [x] 8.3 Teste gerando o DANFE de um XML autorizado de exemplo

## 9. Testes

- [x] 9.1 Emissão com destinatário completo monta o XML no modelo 55, com `dest` preenchido
- [x] 9.2 Cada recusa da seção 5 tem teste próprio, conferindo a mensagem
- [x] 9.3 Transporte, volumes e cobrança presentes no XML quando informados, ausentes quando não
- [x] 9.4 **Regressão:** NFC-e continua no modelo 65, com impressão de cupom, consumidor anônimo, totais zerados e CSC aceito. O QR Code fica de fora: é aplicado depois da montagem, a partir do certificado, e não há como exercitá-lo sem um PFX real
- [x] 9.6 Suíte roda em contêiner Linux, não só no Windows de desenvolvimento
- [x] 9.5 `dotnet test` verde

## 10. Documentação

- [x] 10.1 `AGENTS.md`: contrato da NF-e, o que difere da NFC-e e o recorte vigente
- [x] 10.2 `docs/`: campos da requisição de NF-e e as recusas
- [x] 10.3 Publicar o contrato para a change irmã do backend
