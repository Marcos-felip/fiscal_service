## 1. Contrato

- [x] 1.1 `ImpostoDto` com `IcmsDto`, `IpiDto`, `PisDto` e `CofinsDto`
- [x] 1.2 `IcmsDto`: situação, origem, `modBC`, `vBC`, `pICMS`, `vICMS`, `modBCST`, `pMVAST`, `vBCST`, `pICMSST`, `vICMSST`, `vBCSTRet`, `vICMSSTRet`, `pFCP`, `vFCP`, `pCredSN`, `vCredICMSSN` — mais `pRedBC`, `pRedBCST`, `vBCFCPST`, `pFCPST` e `vFCPST`, que os grupos ICMS20/70 e os de ST exigem
- [x] 1.3 `PisDto` e `CofinsDto` com as duas formas: percentual (`vBC` + `pPIS`) e quantidade (`qBCProd` + `vAliqProd`)
- [x] 1.4 `ItemDto` recebe `Imposto` — **obrigatório**, sem fallback (ver 4.3)
- [x] 1.5 Documentar no `AGENTS.md` do motor o contrato novo — também em `.claude/CLAUDE.md`, que duplica o arquivo, e em `docs/CONTRATO_TRIBUTARIO.md`

## 2. Domínio

- [x] 2.1 Value objects de situação tributária para ICMS (CST e CSOSN), IPI, PIS e COFINS
- [x] 2.2 Tabela declarando, por situação, quais campos são obrigatórios — a regra num lugar só, fora do adapter
- [x] 2.3 Validação de coerência: base sem alíquota, valor divergente de base × alíquota, ST sem campos de ST
- [x] 2.4 Tolerância de centavos igual à já usada nos somatórios

## 3. Tradução para o XML

- [x] 3.1 Reescrever `MontarImposto` como tradutor, sem escolher CST
- [x] 3.2 ICMS do Simples: ICMSSN101, 102, 201, 202, 203, 500 e 900
- [x] 3.3 ICMS do Regime Normal: ICMS00, 10, 20, 30, 40, 51, 60, 70, 90
- [x] 3.4 Grupos de ST e FCP — partilha interestadual fica de fora: a NFC-e deste serviço é sempre operação interna (`idDest` fixo em interna, CFOP validado como 5xxx), então `ICMSPart`/`ICMSUFDest` não têm como ocorrer
- [x] 3.5 IPI, quando informado
- [x] 3.6 PIS e COFINS pelas duas formas, **removendo o CST 07 fixo**
- [x] 3.7 `ValidacoesFiscais` deixa de manter lista curta de CST/CSOSN suportado

## 4. Compatibilidade

- [x] 4.1 ~~Item sem `imposto` continua resolvido pela regra anterior~~ — descartado em 4.3
- [x] 4.2 ~~Log de nível informativo registrando uso do contrato antigo~~ — descartado em 4.3
- [x] 4.3 **Última tarefa da change:** remover o fallback e tornar `imposto` obrigatório

  > O fallback chegou a ser implementado (4.1 e 4.2) e foi removido na sequência: o backend
  > passou a publicar o bloco `imposto` na mesma janela, então não havia ninguém no caminho
  > antigo para proteger — a confirmação por log deixou de ser necessária.
  >
  > Saíram: `TradutorImpostoContratoAntigo`, `RegistrarUsoDoContratoAntigo` no adapter, o
  > parâmetro `crt` de `MontarDetalhe`, `ItemNfceDto.Origem`/`Csosn` e
  > `NfceItem.Origem`/`Csosn`. `ItemNfceDto.Imposto` e `NfceItem.Imposto` passaram a ser
  > obrigatórios.

## 5. Testes — os primeiros do repositório

- [x] 5.1 Criar a estrutura em `tests/FiscalService.UnitTests` (hoje só existe andaime)
- [x] 5.2 Um teste por situação tributária suportada, conferindo o grupo XML gerado
- [x] 5.3 Quadro incoerente recusado, com a mensagem certa: falta de campo, valor divergente, ST incompleta
- [x] 5.4 PIS/COFINS por quantidade
- [x] 5.5 **Regressão:** CSOSN 102 sem valores gera XML equivalente ao de hoje — fixado como XML dourado em `RegressaoEmissaoAtualTests`, não por comparação com outra função do próprio motor
- [x] 5.6 Item sem bloco tributário é recusado — o fallback que esta tarefa cobria saiu em 4.3
- [x] 5.7 `dotnet test` verde

## 6. Handoff

- [x] 6.1 Publicar o contrato novo para a change irmã do backend — `docs/CONTRATO_TRIBUTARIO.md`. O repositório do backend não está clonado nesta máquina (`E:/marco/Documents/gestao_fiscal_backend` só tem `docker/`), então o documento fica aqui para a change irmã consumir
- [x] 6.2 Registrar em `README.md`/`AGENTS.md` que o motor não decide mais imposto — quem decide é o backend
