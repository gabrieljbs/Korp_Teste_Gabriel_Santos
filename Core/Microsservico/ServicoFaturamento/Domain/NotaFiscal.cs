namespace ServicoFaturamento.Domain;

public sealed class NotaFiscal
{
    private readonly List<ItemNotaFiscal> _itens = [];

    // Construtor sem parâmetros necessário para o EF Core (rehydrate da base)
    private NotaFiscal() { }

    /// <summary>Cria uma nova nota fiscal. O Numero será atribuído pelo banco via identity column.</summary>
    public static NotaFiscal Criar() => new() { Status = StatusNotaFiscal.Aberta };

    public long Numero { get; private set; }
    public StatusNotaFiscal Status { get; private set; }
    public IReadOnlyCollection<ItemNotaFiscal> Itens => _itens.AsReadOnly();

    public void AdicionarItem(string codigoProduto, string descricaoProduto, decimal quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");

        ValidarAberta();

        var itemExistente = _itens.FirstOrDefault(item =>
            item.CodigoProduto.Equals(codigoProduto, StringComparison.OrdinalIgnoreCase));

        if (itemExistente is null)
        {
            _itens.Add(new ItemNotaFiscal(Numero, codigoProduto, descricaoProduto, quantidade));
        }
        else
        {
            itemExistente.AdicionarQuantidade(quantidade);
        }
    }

    public void Fechar()
    {
        ValidarAberta();

        if (_itens.Count == 0)
            throw new InvalidOperationException("Não é possível fechar uma nota sem itens.");

        Status = StatusNotaFiscal.Fechada;
    }

    private void ValidarAberta()
    {
        if (Status != StatusNotaFiscal.Aberta)
            throw new InvalidOperationException("A nota fiscal não está aberta para alteração.");
    }
}

public enum StatusNotaFiscal
{
    Aberta = 1,
    Fechada = 2
}
