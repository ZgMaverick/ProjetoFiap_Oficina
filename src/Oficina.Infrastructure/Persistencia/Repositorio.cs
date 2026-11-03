using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Oficina.Application.Abstracoes;

namespace Oficina.Infrastructure.Persistencia;

public class Repositorio<T>(OficinaDbContext db) : IRepositorio<T> where T : class
{
    public Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Set<T>().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);

    public Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro = null, CancellationToken ct = default)
        => (filtro is null ? db.Set<T>() : db.Set<T>().Where(filtro)).ToListAsync(ct);

    public async Task AdicionarAsync(T entidade, CancellationToken ct = default)
        => await db.Set<T>().AddAsync(entidade, ct);

    public void Remover(T entidade) => db.Set<T>().Remove(entidade);
}
