# Modelo de domínio

Este documento descreve o domínio como ele está implementado em `src/Oficina.Domain`, e liga cada conceito ao código.

## Bounded contexts

O MVP é um monólito com um único modelo. Os contextos são separados por namespace dentro do `Oficina.Domain` e do `Oficina.Application`, sem fronteiras físicas.

| Contexto | Tipo | Responsabilidade | Namespaces |
|---|---|---|---|
| **Ordem de Serviço** | Core | Ciclo de vida da OS, orçamento, aprovação, execução, entrega e tempo de execução | `Domain.OrdensDeServico`, `Application.OrdensDeServico` |
| **Estoque** | Suporte | Peças e insumos: saldo físico, reserva, liberação e baixa | `Domain.Estoque`, `Application.Cadastros.PecaInsumoService` |
| **Cadastros** | Suporte | Clientes, veículos e catálogo de serviços | `Domain.Clientes`, `Domain.Veiculos`, `Domain.Servicos`, `Application.Cadastros` |
| **Acesso** | Genérico | Login do usuário administrativo e emissão do JWT | `Api.Controllers.AuthController` |

Os contextos se referenciam apenas pelo `Id`. Por exemplo, a OS guarda o `ClienteId`, o `VeiculoId` e, em cada item, o `ReferenciaId` do serviço ou da peça. Nenhum agregado carrega outro dentro de si.

## Agregados

| Agregado (raiz) | Contém | Invariantes que a raiz garante |
|---|---|---|
| **`OrdemDeServico`** | `ItemOrdemServico` (linhas do orçamento) e `HistoricoStatus` | Só aceita transições de status válidas. Itens só entram em `EmDiagnostico`. O orçamento só é enviado com diagnóstico e ao menos um item. O valor do orçamento é sempre a soma dos itens. |
| **`PecaInsumo`** | — | O saldo disponível nunca fica negativo. A baixa só acontece sobre o reservado. Usa concorrência otimista (`Versao` → coluna `xmin` do PostgreSQL). |
| **`Cliente`** | — | Nome obrigatório. O documento é válido e não muda depois do cadastro. |
| **`Veiculo`** | — | Marca e modelo obrigatórios, ano entre 1886 e o ano seguinte ao atual. A placa não muda depois do cadastro. |
| **`Servico`** | — | Nome obrigatório e preço não negativo. |

**Por que o orçamento está dentro da OS:** aprovar o orçamento e reservar as peças precisam ser consistentes juntos. Com o orçamento como parte do agregado, o total e os itens nunca ficam fora de sincronia com o status da OS.

**Operações que tocam dois agregados**, como incluir uma peça (OS + estoque), recusar, cancelar ou finalizar, são coordenadas pelo `OrdemServicoService` e gravadas numa **única transação** (`IUnidadeDeTrabalho.SalvarAsync`). Se a reserva falhar, nada é gravado.

## Entidades

| Entidade | Identidade | Arquivo |
|---|---|---|
| `OrdemDeServico` | `Id` (Guid) | `src/Oficina.Domain/OrdensDeServico/OrdemDeServico.cs` |
| `ItemOrdemServico` | `Id`, dentro da OS | `src/Oficina.Domain/OrdensDeServico/ItemOrdemServico.cs` |
| `HistoricoStatus` | `Id`, dentro da OS | `src/Oficina.Domain/OrdensDeServico/HistoricoStatus.cs` |
| `PecaInsumo` | `Id`; `Codigo` único | `src/Oficina.Domain/Estoque/PecaInsumo.cs` |
| `Cliente` | `Id`; `Documento` único | `src/Oficina.Domain/Clientes/Cliente.cs` |
| `Veiculo` | `Id`; `Placa` única | `src/Oficina.Domain/Veiculos/Veiculo.cs` |
| `Servico` | `Id` | `src/Oficina.Domain/Servicos/Servico.cs` |

## Objetos de valor

| Objeto de valor | Regra | Arquivo |
|---|---|---|
| `Documento` | CPF (11 dígitos) ou CNPJ (14 dígitos) com dígitos verificadores válidos. Guarda só os dígitos e rejeita sequências repetidas. | `src/Oficina.Domain/Comum/Documento.cs` |
| `Placa` | Formato antigo `ABC1234` ou Mercosul `ABC1D23`. Ignora hífen e espaços e guarda em maiúsculas. | `src/Oficina.Domain/Comum/Placa.cs` |
| `StatusOrdemServico` | `Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada`, `Entregue` e `Cancelada` | `src/Oficina.Domain/OrdensDeServico/StatusOrdemServico.cs` |

Os dois primeiros são `record`, imutáveis e comparados por valor. Uma regra violada lança `DomainException`, que a API devolve como HTTP 400.

## Relação entre documentação e código

| Conceito | Domínio | Caso de uso | Endpoint | Teste |
|---|---|---|---|---|
| Abrir OS identificando cliente e veículo | `OrdemDeServico` (construtor), `Documento`, `Placa` | `OrdemServicoService.CriarAsync` | `POST /api/ordens-servico` | `OrdemServicoServiceTests`, `FluxoOrdemServicoTests` |
| Diagnóstico | `IniciarDiagnostico`, `RegistrarDiagnostico` | `IniciarDiagnosticoAsync`, `RegistrarDiagnosticoAsync` | `POST .../iniciar-diagnostico`, `PUT .../diagnostico` | `OrdemDeServicoTests` |
| Orçamento automático | `AdicionarServico`, `AdicionarPecaInsumo`, `ValorOrcamento` | `AdicionarServicoAsync`, `AdicionarPecaInsumoAsync` | `POST .../servicos`, `POST .../pecas-insumos` | `OrdemDeServicoTests`, `OrdemServicoServiceTests` |
| Envio para aprovação | `EnviarOrcamento` | `EnviarOrcamentoAsync` | `POST .../enviar-orcamento` | `OrdemDeServicoTests` |
| Aprovação ou recusa pelo cliente | `AprovarOrcamento`, `RecusarOrcamento` | `AprovarOrcamentoAsync`, `RecusarOrcamentoAsync`, `ConsultarPeloClienteAsync` | `POST /api/acompanhamento/{id}/aprovar` e `/recusar` | `FluxoOrdemServicoTests` |
| Execução, finalização e entrega | `Finalizar`, `Entregar` | `FinalizarAsync`, `EntregarAsync` | `POST .../finalizar`, `POST .../entregar` | `OrdemDeServicoTests`, `FluxoOrdemServicoTests` |
| Tempo médio de execução | `TempoDeExecucao`, `HistoricoStatus` | `TempoMedioDeExecucaoAsync` | `GET .../tempo-medio-execucao` | `OrdemServicoServiceTests` |
| Estoque | `PecaInsumo` (`RegistrarEntrada`, `Reservar`, `DevolverReserva`, `DarBaixa`) | `PecaInsumoService`, `OrdemServicoService` | `/api/pecas-insumos` | `PecaInsumoTests`, `CadastrosServiceTests` |
| Cadastros | `Cliente`, `Veiculo`, `Servico` | `ClienteService`, `VeiculoService`, `ServicoCatalogoService` | `/api/clientes`, `/api/veiculos`, `/api/servicos` | `CadastrosServiceTests`, `DocumentoEPlacaTests` |

## Decisões do MVP

O modelo do Miro é mais amplo que o código. Estas foram as simplificações:

| Decisão no código | Efeito no modelo do Miro |
|---|---|
| O orçamento faz parte do agregado `OrdemDeServico` | Ficam dois agregados principais (OS e Peça/Insumo) mais os cadastros, e não um agregado Orçamento separado |
| Aprovar o orçamento já coloca a OS em `EmExecucao` | "Orçamento aprovado" e "Execução iniciada" acontecem no mesmo comando |
| A baixa de estoque acontece ao finalizar, para todas as peças da OS | Não há baixa item a item durante a execução |
| A peça é reservada quando entra no orçamento; sem saldo, a inclusão é recusada | Responde em que momento a reserva acontece |
| A consulta e a aprovação do cliente pedem o número da OS e o CPF/CNPJ | Responde como o cliente se identifica sem login |
| Não há orçamento complementar, aprovação parcial, item não concluído nem controle de qualidade | Fora do MVP |
| Não há saldo mínimo, ponto de pedido nem ajuste por contagem | Fora do MVP |
| Não há notificações; o cliente consulta a OS pela API | O quadro de Notificações do Miro fica fora do MVP |
| Os eventos de domínio são registrados no `HistoricoStatus`, e não publicados | Não há barramento de eventos no MVP |
