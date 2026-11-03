namespace Oficina.Domain.OrdensDeServico;

// Cada mudança de status fica registrada com data e hora.
// É daqui que sai o tempo médio de execução.
public class HistoricoStatus
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public StatusOrdemServico Status { get; private set; }
    public DateTime DataHora { get; private set; }

    private HistoricoStatus() { }

    internal HistoricoStatus(StatusOrdemServico status, DateTime dataHora)
    {
        Status = status;
        DataHora = dataHora;
    }
}
