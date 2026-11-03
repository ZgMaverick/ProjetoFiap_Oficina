using Microsoft.EntityFrameworkCore;
using Oficina.Application.Abstracoes;
using Oficina.Domain.Clientes;
using Oficina.Domain.Comum;
using Oficina.Domain.Estoque;
using Oficina.Domain.OrdensDeServico;
using Oficina.Domain.Servicos;
using Oficina.Domain.Veiculos;

namespace Oficina.Infrastructure.Persistencia;

public class OficinaDbContext(DbContextOptions<OficinaDbContext> options)
    : DbContext(options), IUnidadeDeTrabalho
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<PecaInsumo> PecasInsumos => Set<PecaInsumo>();
    public DbSet<OrdemDeServico> OrdensDeServico => Set<OrdemDeServico>();

    // SaveChanges já roda numa transação: ou grava tudo, ou nada.
    public Task SalvarAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Cliente>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedNever();
            e.Property(c => c.Nome).HasMaxLength(150).IsRequired();
            e.Property(c => c.Documento)
                .HasConversion(d => d.Valor, v => new Documento(v))
                .HasMaxLength(14).IsRequired();
            e.HasIndex(c => c.Documento).IsUnique();
        });

        mb.Entity<Veiculo>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).ValueGeneratedNever();
            e.Property(v => v.Placa)
                .HasConversion(p => p.Valor, v => new Placa(v))
                .HasMaxLength(7).IsRequired();
            e.HasIndex(v => v.Placa).IsUnique();
            e.Property(v => v.Marca).HasMaxLength(60);
            e.Property(v => v.Modelo).HasMaxLength(60);
        });

        mb.Entity<Servico>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedNever();
            e.Property(s => s.Nome).HasMaxLength(120).IsRequired();
            e.Property(s => s.Preco).HasPrecision(10, 2);
        });

        mb.Entity<PecaInsumo>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();
            e.Property(p => p.Codigo).HasMaxLength(40).IsRequired();
            e.HasIndex(p => p.Codigo).IsUnique();
            e.Property(p => p.Nome).HasMaxLength(120).IsRequired();
            e.Property(p => p.Unidade).HasMaxLength(10);
            e.Property(p => p.Preco).HasPrecision(10, 2);
            e.Property(p => p.SaldoFisico).HasPrecision(12, 3);
            e.Property(p => p.SaldoReservado).HasPrecision(12, 3);
            e.Ignore(p => p.SaldoDisponivel);

            // Controle de concorrência: no PostgreSQL vira a coluna de sistema xmin.
            // O banco em memória dos testes não tem xmin, então lá a propriedade é ignorada.
            if (Database.IsNpgsql())
                e.Property(p => p.Versao).IsRowVersion();
            else
                e.Ignore(p => p.Versao);
        });

        mb.Entity<OrdemDeServico>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).ValueGeneratedNever();
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(o => o.RelatoCliente).HasMaxLength(1000);
            e.Property(o => o.Diagnostico).HasMaxLength(2000);
            e.Ignore(o => o.ValorOrcamento);

            // Itens e histórico pertencem à OS: são salvos e carregados junto com ela.
            e.OwnsMany(o => o.Itens, i =>
            {
                i.ToTable("ItensOrdemServico");
                i.WithOwner().HasForeignKey("OrdemDeServicoId");
                i.HasKey(x => x.Id);
                i.Property(x => x.Id).ValueGeneratedNever();
                i.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
                i.Property(x => x.Descricao).HasMaxLength(150);
                i.Property(x => x.Quantidade).HasPrecision(12, 3);
                i.Property(x => x.PrecoUnitario).HasPrecision(10, 2);
                i.Ignore(x => x.Total);
            });

            e.OwnsMany(o => o.Historico, h =>
            {
                h.ToTable("HistoricoStatusOrdemServico");
                h.WithOwner().HasForeignKey("OrdemDeServicoId");
                h.HasKey(x => x.Id);
                h.Property(x => x.Id).ValueGeneratedNever();
                h.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            });
        });
    }
}
