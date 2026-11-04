# Linguagem ubíqua

São os termos usados pela oficina, pela documentação e pelo código do MVP, com o nome exato de cada um no código. O glossário completo do domínio, mais amplo que o MVP, está em [`glossario.pdf`](glossario.pdf).

## Atendimento e Ordem de Serviço

| Termo | Definição | No código |
|---|---|---|
| Ordem de Serviço (OS) | Registro central do atendimento, da recepção à entrega. | `OrdemDeServico` |
| Cliente | Quem autoriza o serviço e responde pelo pagamento. Identificado por CPF ou CNPJ. | `Cliente` |
| Documento | CPF ou CNPJ válido do cliente. | `Documento` (objeto de valor) |
| Veículo | O bem atendido, identificado pela placa no MVP. | `Veiculo` |
| Placa | Identificação do veículo, no padrão antigo ou Mercosul. | `Placa` (objeto de valor) |
| Relato do cliente | Descrição do problema nas palavras do cliente, sem interpretação técnica. | `OrdemDeServico.RelatoCliente` |
| Diagnóstico | Investigação técnica que identifica a causa do relato. | `OrdemDeServico.Diagnostico`, `RegistrarDiagnostico()` |
| Status da OS | Etapa em que a OS está. | `StatusOrdemServico` |
| Histórico de status | Registro de cada mudança de status com data e hora. | `HistoricoStatus` |
| Acompanhamento | Consulta da OS pelo próprio cliente, sem login. | `AcompanhamentoController`, `ConsultarPeloClienteAsync()` |

## Orçamento

| Termo | Definição | No código |
|---|---|---|
| Orçamento | Os serviços e peças propostos para a OS, com o valor total. Faz parte da OS. | `OrdemDeServico.Itens` + `ValorOrcamento` |
| Item do orçamento | Uma linha do orçamento: um serviço ou uma peça/insumo, com quantidade e preço unitário. | `ItemOrdemServico`, `TipoItem` |
| Enviar orçamento | Disponibilizar o orçamento ao cliente para aprovação. | `EnviarOrcamento()` → `AguardandoAprovacao` |
| Aprovação | Autorização do cliente para executar o orçamento inteiro. | `AprovarOrcamento()` → `EmExecucao` |
| Recusa | O cliente não autoriza o orçamento; a OS é cancelada. | `RecusarOrcamento()` → `Cancelada` |

## Execução

| Termo | Definição | No código |
|---|---|---|
| Serviço | Trabalho do catálogo da oficina (troca de óleo, alinhamento), com preço e tempo estimado. | `Servico` |
| Execução | Etapa em que os serviços do orçamento aprovado são realizados. | `StatusOrdemServico.EmExecucao` |
| Finalização | Fim da execução; as peças saem do estoque. | `Finalizar()` → `Finalizada` |
| Entrega | Devolução do veículo ao cliente. | `Entregar()` → `Entregue` |
| Tempo de execução | Intervalo entre o início da execução e a finalização de uma OS. | `TempoDeExecucao()` |
| Tempo médio de execução | Média do tempo de execução das OS concluídas. | `TempoMedioDeExecucaoAsync()` |

## Estoque de peças e insumos

| Termo | Definição | No código |
|---|---|---|
| Peça / Insumo | Produto do estoque, identificado por um código único. | `PecaInsumo` |
| Saldo físico | Quantidade que está fisicamente na prateleira. | `SaldoFisico` |
| Saldo reservado | Parte do saldo físico vinculada a OS em andamento. | `SaldoReservado` |
| Saldo disponível | Saldo físico menos saldo reservado. É o que vale para orçar. | `SaldoDisponivel` |
| Entrada de estoque | Recebimento de mercadoria, que aumenta o saldo físico. | `RegistrarEntrada()` |
| Reserva de peça | Vínculo de uma quantidade a uma OS, feito quando a peça entra no orçamento. | `Reservar()` |
| Liberação de reserva | Desfazer uma reserva quando o orçamento é recusado ou a OS é cancelada. | `DevolverReserva()` |
| Baixa de estoque | Saída da peça do saldo físico, ao finalizar a OS. | `DarBaixa()` |

## Termos a evitar

| Evitar | Use |
|---|---|
| "atendimento", "pedido" para a OS | Ordem de Serviço |
| "proposta", "cotação" para o orçamento | Orçamento |
| "estoque" sem qualificação | Saldo físico, reservado ou disponível |
| "cancelamento de reserva" | Liberação de reserva |
| "defeito", "problema" para o que o cliente conta | Relato do cliente |
