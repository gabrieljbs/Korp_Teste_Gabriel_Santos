using ServicoFaturamento.Application;
using ServicoFaturamento.Domain;
using ServicoFaturamento.DTOs;

namespace ServicoFaturamento.Services;

public sealed class FaturamentoService
{
    private readonly INotaFiscalRepository _repositorio;

    public FaturamentoService(INotaFiscalRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<NotaFiscalDto> CriarNotaFiscalAsync(
        CancellationToken cancellationToken = default)
    {
        // O Numero é gerado pelo banco via identity column.
        var nota = NotaFiscal.Criar();
        await _repositorio.AdicionarAsync(nota, cancellationToken);
        await _repositorio.SalvarAsync(cancellationToken);
        return ConverterParaDto(nota);
    }

    public async Task<IReadOnlyCollection<NotaFiscalDto>> ListarNotasFiscaisAsync(
        CancellationToken cancellationToken = default)
    {
        var notas = await _repositorio.ListarAsync(cancellationToken);
        return notas.Select(ConverterParaDto).ToArray();
    }

    public async Task<NotaFiscalDto> ObterNotaFiscalAsync(
        long numero,
        CancellationToken cancellationToken = default)
    {
        var nota = await ObterEntidadeAsync(numero, cancellationToken);
        return ConverterParaDto(nota);
    }

    public async Task<NotaFiscalDto> AdicionarItemAsync(
        long numero,
        string codigoProduto,
        string descricaoProduto,
        decimal quantidade,
        CancellationToken cancellationToken = default)
    {
        var nota = await ObterEntidadeAsync(numero, cancellationToken);
        nota.AdicionarItem(codigoProduto, descricaoProduto, quantidade);
        await _repositorio.SalvarAsync(cancellationToken);
        return ConverterParaDto(nota);
    }

    public async Task<NotaFiscalDto> FecharNotaFiscalAsync(
        long numero,
        CancellationToken cancellationToken = default)
    {
        var nota = await ObterEntidadeAsync(numero, cancellationToken);
        nota.Fechar();
        await _repositorio.SalvarAsync(cancellationToken);
        return ConverterParaDto(nota);
    }

    private async Task<NotaFiscal> ObterEntidadeAsync(
        long numero,
        CancellationToken cancellationToken)
    {
        return await _repositorio.ObterPorNumeroAsync(numero, cancellationToken)
            ?? throw new KeyNotFoundException("Nota fiscal não encontrada.");
    }

    private static NotaFiscalDto ConverterParaDto(NotaFiscal nota) =>
        new()
        {
            Numero = nota.Numero,
            Status = nota.Status,
            Itens = nota.Itens.Select(item => new ItemNotaFiscalDto
            {
                CodigoProduto = item.CodigoProduto,
                DescricaoProduto = item.DescricaoProduto,
                Quantidade = item.Quantidade
            }).ToArray()
        };
}