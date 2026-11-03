using System.Linq.Expressions;

namespace Oficina.Application.Abstracoes;

// Um repositório genérico basta para o MVP: todas as entidades usam Guid Id.
public interface IRepositorio<T> where T : class
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro = null, CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    void Remover(T entidade);
}
