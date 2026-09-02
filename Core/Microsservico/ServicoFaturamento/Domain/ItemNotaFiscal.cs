namespace ServicoFaturamento.Domain;

public sealed class ItemNotaFiscal
{
    private ItemNotaFiscal() { }

    public ItemNotaFiscal(long notaFiscalNumero, string codigoProduto, string descricaoProduto, decimal quantidade)
    {
        NotaFiscalNumero = notaFiscalNumero;
        CodigoProduto = codigoProduto;
        DescricaoProduto = descricaoProduto;
        Quantidade = quantidade;
    }

    public long NotaFiscalNumero { get; private set; }
    public string CodigoProduto { get; private set; } = string.Empty;
    public string DescricaoProduto { get; private set; } = string.Empty;
    public decimal Quantidade { get; private set; }

    public void AdicionarQuantidade(decimal quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");

        Quantidade += quantidade;
    }
}
