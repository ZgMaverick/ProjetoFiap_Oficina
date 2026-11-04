# Event Storming (como implementado)

Versão em texto do Event Storming, limitada ao que o MVP implementa. O quadro completo, com as partes fora do MVP, está no [Miro](https://miro.com/app/board/uXjVEXtrgrQ=/?share_link_id=578425858571) e em [`event-storming.png`](event-storming.png).

**Legenda:** **Ator** → **Comando** → **Agregado** → **Evento**, mais as **Políticas** (regras que o comando precisa respeitar ou que disparam outra ação).

**Atores:**
- **Consultor técnico:** usuário administrativo, autenticado com JWT.
- **Mecânico:** no MVP usa o mesmo usuário administrativo.
- **Cliente:** usa a API pública de acompanhamento, informando o número da OS e o CPF/CNPJ.

> Os eventos de domínio não são classes publicadas num barramento. Cada mudança de status fica registrada em `HistoricoStatus` (status + data/hora), que é o registro dos eventos da OS e a base do cálculo de tempo médio.

---

## 1. Criação da Ordem de Serviço

| Ator | Comando | Agregado | Evento | Políticas / regras |
|---|---|---|---|---|
| Consultor técnico | Abrir OS (`POST /api/ordens-servico`) | Cliente, Veículo, OrdemDeServico | **Cliente identificado** ou **Cliente cadastrado** | O cliente é identificado pelo CPF/CNPJ. Se não existir, é cadastrado na hora. O documento precisa ter dígitos verificadores válidos (`Documento`). |
| | | | **Veículo identificado** ou **Veículo cadastrado** | O veículo é identificado pela placa. Se não existir, é cadastrado com marca, modelo e ano. A placa precisa estar no padrão antigo ou Mercosul (`Placa`). O ano vai de 1886 até o ano seguinte ao atual. |
| | | | **OS recebida** | O relato do cliente é obrigatório. Toda OS nasce `Recebida`. |
| Mecânico | Iniciar diagnóstico | OrdemDeServico | **Diagnóstico iniciado** | Só a partir de `Recebida`. |
| Mecânico | Registrar diagnóstico | OrdemDeServico | **Diagnóstico registrado** | Só em `EmDiagnostico`. O texto não pode ser vazio. |
| Consultor técnico | Incluir serviço | OrdemDeServico | **Serviço incluído no orçamento** | Só em `EmDiagnostico`. O preço é copiado do catálogo no momento da inclusão. |
| Consultor técnico | Incluir peça/insumo | OrdemDeServico + PecaInsumo | **Peça incluída no orçamento** e **Peça reservada** | Só em `EmDiagnostico`. A quantidade deve ser maior que zero. A peça é reservada na mesma transação, e sem saldo disponível a inclusão é recusada (400). |
| Consultor técnico | Enviar orçamento | OrdemDeServico | **Orçamento enviado** (OS `AguardandoAprovacao`) | Exige diagnóstico registrado e pelo menos um item. O valor do orçamento é a soma dos itens, calculada automaticamente. |

## 2. Acompanhamento da Ordem de Serviço

| Ator | Comando | Agregado | Evento | Políticas / regras |
|---|---|---|---|---|
| Cliente | Consultar OS (`GET /api/acompanhamento/{id}?documento=`) | OrdemDeServico | — | O cliente só vê a OS se informar o CPF/CNPJ do dono. Se não for o dono, a resposta é 404, sem revelar que a OS existe. |
| Cliente | Aprovar orçamento | OrdemDeServico | **Orçamento aprovado** e **Execução iniciada** (`EmExecucao`) | Só em `AguardandoAprovacao`. A aprovação já inicia a execução. |
| Cliente | Recusar orçamento | OrdemDeServico + PecaInsumo | **Orçamento recusado** (OS `Cancelada`) e **Reserva liberada** | Só em `AguardandoAprovacao`. As peças reservadas voltam ao saldo disponível. |
| Consultor técnico | Cancelar OS | OrdemDeServico + PecaInsumo | **OS cancelada** e **Reserva liberada** | Não pode cancelar uma OS em execução, finalizada, entregue ou já cancelada. |
| Mecânico | Finalizar OS | OrdemDeServico + PecaInsumo | **OS finalizada** e **Baixa de estoque** | Só em `EmExecucao`. Dá baixa de todas as peças da OS sobre o que estava reservado. |
| Consultor técnico | Entregar veículo | OrdemDeServico | **Veículo entregue** (`Entregue`) | Só a partir de `Finalizada`. |
| Consultor técnico | Consultar tempo médio (`GET /api/ordens-servico/tempo-medio-execucao`) | OrdemDeServico | — | Tempo médio é a média, nas OS finalizadas ou entregues, do intervalo entre `EmExecucao` e `Finalizada` registrado no histórico. |

Fluxo de status (as transições são aplicadas pelo agregado; qualquer outra responde 400):

```
Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue

Cancelada ← Cancelar (a partir de Recebida, EmDiagnostico ou AguardandoAprovacao)
Cancelada ← Recusar orçamento (a partir de AguardandoAprovacao)
```

## 3. Gestão de peças e insumos

| Ator | Comando | Agregado | Evento | Políticas / regras |
|---|---|---|---|---|
| Consultor técnico | Cadastrar peça/insumo | PecaInsumo | **Peça cadastrada** | Código obrigatório e único, guardado em maiúsculas. Nome obrigatório, preço não negativo, unidade padrão `un`. |
| Consultor técnico | Registrar entrada (`POST /api/pecas-insumos/{id}/entrada`) | PecaInsumo | **Entrada de estoque registrada** | Quantidade maior que zero. Aumenta o saldo físico. |
| (pela OS) | Reservar | PecaInsumo | **Peça reservada** | Só reserva até o saldo disponível (físico − reservado). Se duas OS reservarem a última peça ao mesmo tempo, a segunda recebe 409 (concorrência otimista). |
| (pela OS) | Liberar reserva | PecaInsumo | **Reserva liberada** | Não libera mais do que está reservado. |
| (pela OS) | Dar baixa | PecaInsumo | **Baixa de estoque** | A baixa só acontece sobre quantidade reservada e reduz o físico e o reservado. |
| Consultor técnico | Remover peça | PecaInsumo | **Peça removida** | Não remove uma peça com reserva ativa. |

---

## Resumo

### Atores

| Ator | Como acessa | O que faz |
|---|---|---|
| Consultor técnico | API administrativa com JWT | Abre a OS, monta e envia o orçamento, cancela, entrega, mantém cadastros e estoque, consulta o tempo médio |
| Mecânico | API administrativa com JWT (mesmo usuário no MVP) | Diagnostica e finaliza a OS |
| Cliente | API pública `/api/acompanhamento`, com número da OS + CPF/CNPJ | Consulta a OS, aprova ou recusa o orçamento |

### Comandos

| Comando | Método no domínio | Endpoint |
|---|---|---|
| Abrir OS | `new OrdemDeServico(...)` | `POST /api/ordens-servico` |
| Iniciar diagnóstico | `IniciarDiagnostico()` | `POST /api/ordens-servico/{id}/iniciar-diagnostico` |
| Registrar diagnóstico | `RegistrarDiagnostico()` | `PUT /api/ordens-servico/{id}/diagnostico` |
| Incluir serviço | `AdicionarServico()` | `POST /api/ordens-servico/{id}/servicos` |
| Incluir peça/insumo | `AdicionarPecaInsumo()` + `PecaInsumo.Reservar()` | `POST /api/ordens-servico/{id}/pecas-insumos` |
| Enviar orçamento | `EnviarOrcamento()` | `POST /api/ordens-servico/{id}/enviar-orcamento` |
| Aprovar orçamento | `AprovarOrcamento()` | `POST /api/acompanhamento/{id}/aprovar` |
| Recusar orçamento | `RecusarOrcamento()` + `DevolverReserva()` | `POST /api/acompanhamento/{id}/recusar` |
| Cancelar OS | `Cancelar()` + `DevolverReserva()` | `POST /api/ordens-servico/{id}/cancelar` |
| Finalizar OS | `Finalizar()` + `DarBaixa()` | `POST /api/ordens-servico/{id}/finalizar` |
| Entregar veículo | `Entregar()` | `POST /api/ordens-servico/{id}/entregar` |
| Cadastrar peça/insumo | `new PecaInsumo(...)` | `POST /api/pecas-insumos` |
| Registrar entrada de estoque | `RegistrarEntrada()` | `POST /api/pecas-insumos/{id}/entrada` |

### Eventos de domínio

| Evento | Quando acontece | Onde fica registrado |
|---|---|---|
| OS recebida | Abertura da OS | `HistoricoStatus` (`Recebida`) |
| Cliente cadastrado / Veículo cadastrado | Abertura de OS com CPF/CNPJ ou placa novos | Tabelas de clientes e veículos |
| Diagnóstico iniciado | Iniciar diagnóstico | `HistoricoStatus` (`EmDiagnostico`) |
| Diagnóstico registrado | Registrar diagnóstico | `OrdemDeServico.Diagnostico` |
| Serviço incluído no orçamento | Incluir serviço | `ItemOrdemServico` |
| Peça incluída no orçamento | Incluir peça/insumo | `ItemOrdemServico` |
| Peça reservada | Incluir peça/insumo | `PecaInsumo.SaldoReservado` |
| Orçamento enviado | Enviar orçamento | `HistoricoStatus` (`AguardandoAprovacao`) |
| Orçamento aprovado / Execução iniciada | Aprovar orçamento | `HistoricoStatus` (`EmExecucao`) |
| Orçamento recusado | Recusar orçamento | `HistoricoStatus` (`Cancelada`) |
| OS cancelada | Cancelar OS | `HistoricoStatus` (`Cancelada`) |
| Reserva liberada | Recusar orçamento ou cancelar OS | `PecaInsumo.SaldoReservado` |
| OS finalizada | Finalizar OS | `HistoricoStatus` (`Finalizada`) |
| Baixa de estoque | Finalizar OS | `PecaInsumo.SaldoFisico` e `SaldoReservado` |
| Veículo entregue | Entregar veículo | `HistoricoStatus` (`Entregue`) |
| Entrada de estoque registrada | Registrar entrada | `PecaInsumo.SaldoFisico` |

### Políticas (regras de negócio)

| Política | Implementação |
|---|---|
| Cliente identificado por CPF/CNPJ válido; se não existir, é cadastrado | `Documento`, `OrdemServicoService.CriarAsync` |
| Veículo identificado por placa válida; se não existir, é cadastrado | `Placa`, `OrdemServicoService.CriarAsync` |
| Cada comando só vale no status certo; transição inválida responde 400 | `OrdemDeServico.ExigirStatus` |
| Toda mudança de status é registrada com data e hora | `OrdemDeServico.MudarStatus` → `HistoricoStatus` |
| Itens só entram no orçamento durante o diagnóstico | `AdicionarServico`, `AdicionarPecaInsumo` |
| Ao incluir uma peça, ela é reservada; sem saldo disponível, a inclusão é recusada | `PecaInsumo.Reservar` |
| O orçamento só é enviado com diagnóstico e ao menos um item | `EnviarOrcamento` |
| O valor do orçamento é sempre a soma dos itens | `ValorOrcamento` |
| Aprovar o orçamento inicia a execução | `AprovarOrcamento` |
| Recusar o orçamento cancela a OS e libera as reservas | `RecusarOrcamentoAsync` |
| Não se cancela uma OS em execução, finalizada ou entregue | `Cancelar` |
| Ao finalizar, todas as peças da OS saem do estoque | `FinalizarAsync` → `DarBaixa` |
| O cliente só vê ou aprova a própria OS | `ConsultarPeloClienteAsync` |
| Duas OS não reservam a mesma última peça | Concorrência otimista (`Versao`/`xmin`) → 409 |
| Comandos que alteram OS e estoque são gravados numa transação | `IUnidadeDeTrabalho.SalvarAsync` |
| Não se remove peça com reserva ativa | `PecaInsumoService.RemoverAsync` |
