using Microsoft.AspNetCore.Mvc;
using ServicoEstoque.Services;

namespace ServicoEstoque.Controllers;

[ApiController]
[Route("api/estoque")]
public sealed class ProdutosController : ControllerBase
{
    private readonly EstoqueService _estoqueService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(EstoqueService estoqueService, ILogger<ProdutosController> logger)
    {
        _estoqueService = estoqueService;
        _logger = logger;
    }

    /// <summary>Cadastra um novo produto no estoque.</summary>
    [HttpPost("produtos")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoResponse>> CadastrarProduto(
        [FromBody] CadastrarProdutoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _estoqueService.CadastrarProdutoAsync(
                request.Codigo, request.Descricao, request.SaldoInicial, cancellationToken);

            var resposta = ProdutoResponse.De(dto);
            return CreatedAtAction(nameof(ObterProdutoPorCodigo), new { codigo = dto.Codigo }, resposta);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflito ao cadastrar produto {Codigo}.", request.Codigo);
            return Conflict(new ErroResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido ao cadastrar produto.");
            return BadRequest(new ErroResponse(ex.Message));
        }
    }

    /// <summary>Lista todos os produtos cadastrados.</summary>
    [HttpGet("produtos")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ProdutoResponse>>> ListarProdutos(
        CancellationToken cancellationToken = default)
    {
        var dtos = await _estoqueService.ListarProdutosAsync(cancellationToken);
        return Ok(dtos.Select(ProdutoResponse.De).ToArray());
    }

    /// <summary>Obtém um produto pelo código.</summary>
    [HttpGet("produtos/{codigo}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> ObterProdutoPorCodigo(
        string codigo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _estoqueService.ObterProdutoPorCodigoAsync(codigo, cancellationToken);
            return Ok(ProdutoResponse.De(dto));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Altera o saldo de um produto.
    /// Use quantidade positiva para crédito e negativa para débito.
    /// </summary>
    [HttpPatch("produtos/{codigo}/saldo")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> AlterarSaldo(
        string codigo,
        [FromBody] AlterarSaldoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _estoqueService.AlterarSaldoAsync(codigo, request.Quantidade, cancellationToken);
            return Ok(ProdutoResponse.De(dto));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Saldo insuficiente para o produto {Codigo}.", codigo);
            return BadRequest(new ErroResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErroResponse(ex.Message));
        }
    }
}

// ── Request / Response Records ─────────────────────────────────────────────

/// <summary>Payload para cadastro de produto.</summary>
public sealed record CadastrarProdutoRequest
{
    /// <example>P001</example>
    public required string Codigo { get; init; }

    /// <example>Parafuso M6</example>
    public required string Descricao { get; init; }

    /// <example>100</example>
    public decimal SaldoInicial { get; init; }
}

/// <summary>Payload para alteração de saldo.</summary>
public sealed record AlterarSaldoRequest
{
    /// <summary>Quantidade a ajustar. Use valores negativos para débito.</summary>
    /// <example>-5</example>
    public decimal Quantidade { get; init; }
}

/// <summary>Representação pública de um produto.</summary>
public sealed record ProdutoResponse
{
    public required string Codigo { get; init; }
    public required string Descricao { get; init; }
    public decimal Saldo { get; init; }

    internal static ProdutoResponse De(ServicoEstoque.DTOs.ProdutoDto dto) => new()
    {
        Codigo = dto.Codigo,
        Descricao = dto.Descricao,
        Saldo = dto.Saldo
    };
}

/// <summary>Resposta padronizada de erro.</summary>
public sealed record ErroResponse(string Mensagem);
