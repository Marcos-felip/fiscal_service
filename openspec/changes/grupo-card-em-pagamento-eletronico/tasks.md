## 1. Regra

- [x] 1.1 Lista das formas de pagamento que exigem o grupo `card`, com a fonte no comentário
- [x] 1.2 `tpIntegra` fixo em "não integrado" — e o porquê registrado no código

## 2. Montagem

- [x] 2.1 `detPag` monta o grupo `card` para as formas eletrônicas
- [x] 2.2 Formas não eletrônicas continuam sem o grupo
- [x] 2.3 Vale para os dois modelos: a montagem dos pagamentos é compartilhada

## 3. Testes

- [x] 3.1 PIX gera `<card><tpIntegra>2</tpIntegra></card>` — o caso que rejeitou em produção de teste
- [x] 3.2 Cartão de crédito e débito geram o grupo
- [x] 3.3 Dinheiro **não** gera o grupo
- [x] 3.4 Cada forma da tabela cai do lado certo
- [x] 3.5 **Regressão:** o XML da NFC-e em dinheiro não muda
- [x] 3.6 `dotnet test` verde

## 4. Fechamento

- [x] 4.1 Documentar em `AGENTS.md` que pagamento eletrônico leva o grupo
- [x] 4.2 Reconstruir a imagem do motor — `dotnet build` no host não atualiza o contêiner
- [x] 4.3 **Autorizada em 14/08/2026:** NFC-e nº 4 em PIX, chave `31260851720322000146650010000000041679548502`, protocolo `131260000764989`. O XML saiu com `tPag` 17 e `<card><tpIntegra>2</tpIntegra></card>`
