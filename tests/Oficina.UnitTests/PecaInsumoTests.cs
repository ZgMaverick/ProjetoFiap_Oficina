using Oficina.Domain.Comum;
using Oficina.Domain.Estoque;

namespace Oficina.UnitTests;

public class PecaInsumoTests
{
    private static PecaInsumo PecaComSaldo(decimal saldo)
    {
        var peca = new PecaInsumo("F-01", "Filtro de óleo", "un", 45m);
        peca.RegistrarEntrada(saldo);
        return peca;
    }

    [Fact]
    public void Reservar_diminui_o_disponivel_sem_mexer_no_fisico()
    {
        var peca = PecaComSaldo(10);
        peca.Reservar(3);

        Assert.Equal(10, peca.SaldoFisico);
        Assert.Equal(3, peca.SaldoReservado);
        Assert.Equal(7, peca.SaldoDisponivel);
    }

    [Fact]
    public void Nao_reserva_mais_que_o_disponivel()
    {
        var peca = PecaComSaldo(2);
        Assert.Throws<DomainException>(() => peca.Reservar(3));
    }

    [Fact]
    public void Baixa_tira_do_fisico_e_do_reservado()
    {
        var peca = PecaComSaldo(10);
        peca.Reservar(4);
        peca.DarBaixa(4);

        Assert.Equal(6, peca.SaldoFisico);
        Assert.Equal(0, peca.SaldoReservado);
    }

    [Fact]
    public void Nao_da_baixa_sem_reserva()
    {
        var peca = PecaComSaldo(10);
        Assert.Throws<DomainException>(() => peca.DarBaixa(1));
    }

    [Fact]
    public void Devolver_reserva_libera_o_disponivel()
    {
        var peca = PecaComSaldo(5);
        peca.Reservar(5);
        peca.DevolverReserva(5);

        Assert.Equal(5, peca.SaldoDisponivel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantidade_precisa_ser_positiva(decimal quantidade)
    {
        var peca = PecaComSaldo(5);
        Assert.Throws<DomainException>(() => peca.Reservar(quantidade));
        Assert.Throws<DomainException>(() => peca.RegistrarEntrada(quantidade));
    }
}
