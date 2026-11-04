using Oficina.Domain.Comum;
using Oficina.Domain.Estoque;
using Oficina.Domain.OrdensDeServico;
using Oficina.Domain.Servicos;

namespace Oficina.UnitTests;

public class OrdemDeServicoTests
{
    private static OrdemDeServico NovaOs() => new(Guid.NewGuid(), Guid.NewGuid(), "Barulho ao frear");

    // Leva a OS até "Aguardando aprovação" com um serviço e uma peça.
    private static OrdemDeServico OsComOrcamentoEnviado()
    {
        var os = NovaOs();
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico("Pastilhas gastas");
        os.AdicionarServico(new Servico("Troca de pastilhas", 120m, 60));
        os.AdicionarPecaInsumo(new PecaInsumo("P-01", "Pastilha", "jogo", 90m), 1);
        os.EnviarOrcamento();
        return os;
    }

    [Fact]
    public void Nova_os_nasce_recebida_e_registra_historico()
    {
        var os = NovaOs();
        Assert.Equal(StatusOrdemServico.Recebida, os.Status);
        Assert.Single(os.Historico);
    }

    [Fact]
    public void Os_sem_relato_e_rejeitada()
        => Assert.Throws<DomainException>(() => new OrdemDeServico(Guid.NewGuid(), Guid.NewGuid(), " "));

    [Fact]
    public void Orcamento_soma_servicos_e_pecas()
    {
        var os = OsComOrcamentoEnviado();
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        Assert.Equal(210m, os.ValorOrcamento);
    }

    [Fact]
    public void Nao_envia_orcamento_sem_itens()
    {
        var os = NovaOs();
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico("Nada encontrado");
        Assert.Throws<DomainException>(() => os.EnviarOrcamento());
    }

    [Fact]
    public void Nao_envia_orcamento_sem_diagnostico()
    {
        var os = NovaOs();
        os.IniciarDiagnostico();
        os.AdicionarServico(new Servico("Alinhamento", 80m, 30));
        Assert.Throws<DomainException>(() => os.EnviarOrcamento());
    }

    [Fact]
    public void Nao_adiciona_item_fora_do_diagnostico()
    {
        var os = NovaOs(); // ainda Recebida
        Assert.Throws<DomainException>(() => os.AdicionarServico(new Servico("Alinhamento", 80m, 30)));
    }

    [Fact]
    public void Fluxo_completo_ate_entregue()
    {
        var os = OsComOrcamentoEnviado();
        os.AprovarOrcamento();
        os.Finalizar();
        os.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
        Assert.Equal(6, os.Historico.Count);
        Assert.NotNull(os.TempoDeExecucao());
    }

    [Fact]
    public void Recusar_orcamento_cancela_a_os()
    {
        var os = OsComOrcamentoEnviado();
        os.RecusarOrcamento();
        Assert.Equal(StatusOrdemServico.Cancelada, os.Status);
    }

    [Fact]
    public void Nao_finaliza_sem_aprovacao()
    {
        var os = OsComOrcamentoEnviado();
        Assert.Throws<DomainException>(() => os.Finalizar());
    }

    [Fact]
    public void Nao_entrega_antes_de_finalizar()
    {
        var os = OsComOrcamentoEnviado();
        os.AprovarOrcamento();
        Assert.Throws<DomainException>(() => os.Entregar());
    }

    [Fact]
    public void Nao_cancela_os_em_execucao()
    {
        var os = OsComOrcamentoEnviado();
        os.AprovarOrcamento();
        Assert.Throws<DomainException>(() => os.Cancelar());
    }

    [Fact]
    public void Tempo_de_execucao_e_nulo_antes_de_finalizar()
    {
        var os = OsComOrcamentoEnviado();
        os.AprovarOrcamento();
        Assert.Null(os.TempoDeExecucao());
    }
}
