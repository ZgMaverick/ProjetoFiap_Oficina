using Oficina.Domain.Comum;

namespace Oficina.UnitTests;

public class DocumentoEPlacaTests
{
    [Theory]
    [InlineData("529.982.247-25")]        // CPF válido com máscara
    [InlineData("52998224725")]           // CPF válido sem máscara
    [InlineData("11.222.333/0001-81")]    // CNPJ válido
    public void Documento_valido_e_aceito(string valor)
    {
        var doc = new Documento(valor);
        Assert.DoesNotContain(".", doc.Valor);
    }

    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    [InlineData("11.222.333/0001-00")]
    [InlineData("123")]
    [InlineData("")]
    public void Documento_invalido_e_rejeitado(string valor)
        => Assert.Throws<DomainException>(() => new Documento(valor));

    [Theory]
    [InlineData("ABC1234")]   // formato antigo
    [InlineData("abc-1234")]  // com hífen e minúsculas
    [InlineData("ABC1D23")]   // Mercosul
    public void Placa_valida_e_aceita(string valor)
        => Assert.Equal(7, new Placa(valor).Valor.Length);

    [Theory]
    [InlineData("AB12345")]
    [InlineData("ABCD123")]
    [InlineData("")]
    public void Placa_invalida_e_rejeitada(string valor)
        => Assert.Throws<DomainException>(() => new Placa(valor));
}
