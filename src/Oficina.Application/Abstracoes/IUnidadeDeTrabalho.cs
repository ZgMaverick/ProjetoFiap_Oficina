namespace Oficina.Application.Abstracoes;

// Salva tudo o que mudou numa única transação.
public interface IUnidadeDeTrabalho
{
    Task SalvarAsync(CancellationToken ct = default);
}
