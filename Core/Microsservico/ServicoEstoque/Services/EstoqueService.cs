using ServicoEstoque.Application;
using ServicoEstoque.Domain;
using ServicoEstoque.DTOs;

namespace ServicoEstoque.Services;

public sealed class EstoqueService
{
    private readonly IProdutoRepository repositorio;

    public EstoqueService(IProdutoRepository repositorio)
    {
        this.repositorio = repositorio;
    }

    public async Task<ProdutoDto> CadastrarProdutoAsync(
        string codigo, string descricao, decimal saldoInicial,
        CancellationToken cancellationToken = default)
    {
        var codigoNormalizado = codigo.Trim();
        var existente = await repositorio.ObterPorCodigoAsync(codigoNormalizado, cancellationToken);

        if (existente is not null)
            throw new InvalidOperationException("Já existe um produto com este código.");

        var produto = new Produto(codigoNormalizado, descricao, saldoInicial);
        await repositorio.AdicionarAsync(produto, cancellationToken);
        await repositorio.SalvarAsync(cancellationToken);
        return ConverterParaDto(produto);
    }

    public async Task<IReadOnlyCollection<ProdutoDto>> ListarProdutosAsync(
        CancellationToken cancellationToken = default)
    {
        var produtos = await repositorio.ListarAsync(cancellationToken);
        return produtos.Select(ConverterParaDto).ToArray();
    }

    public async Task<ProdutoDto> ObterProdutoPorCodigoAsync(
        string codigo, CancellationToken cancellationToken = default)
    {
        return ConverterParaDto(await ObterEntidadePorCodigoAsync(codigo, cancellationToken));
    }

    public async Task<ProdutoDto> AlterarSaldoAsync(
        string codigo, decimal quantidade, CancellationToken cancellationToken = default)
    {
        var produto = await ObterEntidadePorCodigoAsync(codigo, cancellationToken);
        produto.AlterarSaldo(quantidade);
        await repositorio.SalvarAsync(cancellationToken);
        return ConverterParaDto(produto);
    }

    private async Task<Produto> ObterEntidadePorCodigoAsync(
        string codigo, CancellationToken cancellationToken)
    {
        var produto = await repositorio.ObterPorCodigoAsync(codigo.Trim(), cancellationToken);
        return produto ?? throw new KeyNotFoundException("Produto não encontrado.");
    }

    private static ProdutoDto ConverterParaDto(Produto produto)
    {
        return new ProdutoDto
        {
            Codigo = produto.Codigo,
            Descricao = produto.Descricao,
            Saldo = produto.Saldo
        };
    }
}