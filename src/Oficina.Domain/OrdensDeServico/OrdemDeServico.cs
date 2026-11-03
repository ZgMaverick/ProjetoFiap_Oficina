using Oficina.Domain.Comum;
using Oficina.Domain.Estoque;
using Oficina.Domain.Servicos;

namespace Oficina.Domain.OrdensDeServico;

// Raiz do agregado. O orçamento faz parte dela: são os itens + o total.
// Toda mudança de status passa por um método que confere se ela é permitida.
public class OrdemDeServico
{
    private readonly List<ItemOrdemServico> _itens = [];
    private readonly List<HistoricoStatus> _historico = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public string RelatoCliente { get; private set; } = "";
    public string? Diagnostico { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public DateTime CriadaEm { get; private set; }

    public IReadOnlyCollection<ItemOrdemServico> Itens => _itens;
    public IReadOnlyCollection<HistoricoStatus> Historico => _historico;
    public decimal ValorOrcamento => _itens.Sum(i => i.Total);

    private OrdemDeServico() { }

    public OrdemDeServico(Guid clienteId, Guid veiculoId, string relatoCliente)
    {
        if (string.IsNullOrWhiteSpace(relatoCliente))
            throw new DomainException("Descreva o problema relatado pelo cliente.");

        ClienteId = clienteId;
        VeiculoId = veiculoId;
        RelatoCliente = relatoCliente.Trim();
        CriadaEm = DateTime.UtcNow;
        MudarStatus(StatusOrdemServico.Recebida);
    }

    public void IniciarDiagnostico()
    {
        ExigirStatus(StatusOrdemServico.Recebida);
        MudarStatus(StatusOrdemServico.EmDiagnostico);
    }

    public void RegistrarDiagnostico(string diagnostico)
    {
        ExigirStatus(StatusOrdemServico.EmDiagnostico);
        if (string.IsNullOrWhiteSpace(diagnostico))
            throw new DomainException("O diagnóstico não pode ser vazio.");
        Diagnostico = diagnostico.Trim();
    }

    public void AdicionarServico(Servico servico)
    {
        ExigirStatus(StatusOrdemServico.EmDiagnostico);
        _itens.Add(new ItemOrdemServico(TipoItem.Servico, servico.Id, servico.Nome, 1, servico.Preco));
    }

    // A reserva no estoque é feita pela camada de aplicação, na mesma transação.
    public void AdicionarPecaInsumo(PecaInsumo peca, decimal quantidade)
    {
        ExigirStatus(StatusOrdemServico.EmDiagnostico);
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");
        _itens.Add(new ItemOrdemServico(TipoItem.PecaInsumo, peca.Id, peca.Nome, quantidade, peca.Preco));
    }

    public void EnviarOrcamento()
    {
        ExigirStatus(StatusOrdemServico.EmDiagnostico);
        if (Diagnostico is null)
            throw new DomainException("Registre o diagnóstico antes de enviar o orçamento.");
        if (_itens.Count == 0)
            throw new DomainException("O orçamento precisa ter ao menos um item.");
        MudarStatus(StatusOrdemServico.AguardandoAprovacao);
    }

    // Com o orçamento aprovado, a execução começa.
    public void AprovarOrcamento()
    {
        ExigirStatus(StatusOrdemServico.AguardandoAprovacao);
        MudarStatus(StatusOrdemServico.EmExecucao);
    }

    public void RecusarOrcamento()
    {
        ExigirStatus(StatusOrdemServico.AguardandoAprovacao);
        MudarStatus(StatusOrdemServico.Cancelada);
    }

    public void Cancelar()
    {
        if (Status is StatusOrdemServico.EmExecucao or StatusOrdemServico.Finalizada
            or StatusOrdemServico.Entregue or StatusOrdemServico.Cancelada)
            throw new DomainException($"Uma OS com status {Status} não pode ser cancelada.");
        MudarStatus(StatusOrdemServico.Cancelada);
    }

    public void Finalizar()
    {
        ExigirStatus(StatusOrdemServico.EmExecucao);
        MudarStatus(StatusOrdemServico.Finalizada);
    }

    public void Entregar()
    {
        ExigirStatus(StatusOrdemServico.Finalizada);
        MudarStatus(StatusOrdemServico.Entregue);
    }

    // Tempo entre entrar em execução e ser finalizada. Nulo se ainda não terminou.
    public TimeSpan? TempoDeExecucao()
    {
        var inicio = _historico.LastOrDefault(h => h.Status == StatusOrdemServico.EmExecucao);
        var fim = _historico.LastOrDefault(h => h.Status == StatusOrdemServico.Finalizada);
        return inicio is null || fim is null ? null : fim.DataHora - inicio.DataHora;
    }

    public IEnumerable<ItemOrdemServico> ItensDePecas()
        => _itens.Where(i => i.Tipo == TipoItem.PecaInsumo);

    private void ExigirStatus(StatusOrdemServico esperado)
    {
        if (Status != esperado)
            throw new DomainException(
                $"Ação não permitida: a OS está {Status} e precisa estar {esperado}.");
    }

    private void MudarStatus(StatusOrdemServico novo)
    {
        Status = novo;
        _historico.Add(new HistoricoStatus(novo, DateTime.UtcNow));
    }
}
