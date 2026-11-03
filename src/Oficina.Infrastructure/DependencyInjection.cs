using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Abstracoes;
using Oficina.Infrastructure.Persistencia;

namespace Oficina.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestrutura(this IServiceCollection services,
        string connectionString, bool usarBancoEmMemoria = false)
    {
        if (usarBancoEmMemoria)
        {
            // Um nome novo por instância da aplicação: cada execução de teste começa vazia.
            var nomeDoBanco = $"oficina-{Guid.NewGuid()}";
            services.AddDbContext<OficinaDbContext>(o => o.UseInMemoryDatabase(nomeDoBanco));
        }
        else
        {
            services.AddDbContext<OficinaDbContext>(o => o.UseNpgsql(connectionString));
        }

        services.AddScoped<IUnidadeDeTrabalho>(sp => sp.GetRequiredService<OficinaDbContext>());
        services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
        return services;
    }
}
