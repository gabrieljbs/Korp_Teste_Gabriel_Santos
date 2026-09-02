using Microsoft.AspNetCore.Mvc;
using ServicoFaturamento.DTOs;
using ServicoFaturamento.Services;

namespace ServicoFaturamento.Controllers;

[ApiController]
[Route("api/faturamento")]
public sealed class NotasFiscaisController : ControllerBase
{
    private readonly FaturamentoService _faturamentoService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(
        FaturamentoService faturamentoService,
        IHttpClientFactory httpClientFactory,
        ILogger<NotasFiscaisController> logger)
    {
        _faturamentoService = faturamentoService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Cria uma nova nota fiscal em aberto com numeração sequencial.</summary>
    [HttpPost("notas")]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<NotaFiscalResponse>> CriarNotaFiscal(
        CancellationToken cancellationToken = default)
    {
        var nota = await _faturamentoService.CriarNotaFiscalAsync(cancellationToken);
        var resposta = NotaFiscalResponse.De(nota);
        return CreatedAtAction(nameof(ObterNotaFiscal), new { numero = nota.Numero }, resposta);
    }

    /// <summary>Lista todas as notas fiscais em ordem decrescente de número.</summary>
    [HttpGet("notas")]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotaFiscalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<NotaFiscalResponse>>> ListarNotasFiscais(
        CancellationToken cancellationToken = default)
    {
        var notas = await _faturamentoService.ListarNotasFiscaisAsync(cancellationToken);
        return Ok(notas.Select(NotaFiscalResponse.De).ToArray());
    }

    /// <summary>Obtém uma nota fiscal pelo número.</summary>
    [HttpGet("notas/{numero:long}")]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaFiscalResponse>> ObterNotaFiscal(
        long numero,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var nota = await _faturamentoService.ObterNotaFiscalAsync(numero, cancellationToken);
            return Ok(NotaFiscalResponse.De(nota));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Adiciona um item a uma nota fiscal aberta.</summary>
    [HttpPost("notas/{numero:long}/itens")]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaFiscalResponse>> AdicionarItem(
        long numero,
        [FromBody] AdicionarItemRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var nota = await _faturamentoService.AdicionarItemAsync(
                numero,
                request.CodigoProduto,
                request.DescricaoProduto,
                request.Quantidade,
                cancellationToken);

            return Ok(NotaFiscalResponse.De(nota));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new ErroResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErroResponse(ex.Message));
        }
    }

    /// <summary>Remove um item de uma nota fiscal aberta.</summary>
    [HttpDelete("notas/{numero:long}/itens/{codigoProduto}")]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaFiscalResponse>> RemoverItem(
        long numero,
        string codigoProduto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var nota = await _faturamentoService.RemoverItemAsync(numero, codigoProduto, cancellationToken);
            return Ok(NotaFiscalResponse.De(nota));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErroResponse(ex.Message));
        }
    }

    /// <summary>
    /// Fecha uma nota fiscal.
    /// Para cada item, debita o saldo no Serviço de Estoque via HTTP.
    /// Retorna 503 se o Estoque estiver indisponível — a nota NÃO é fechada nesses casos.
    /// </summary>
    [HttpPost("notas/{numero:long}/fechar")]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<NotaFiscalResponse>> FecharNotaFiscal(
        long numero,
        CancellationToken cancellationToken = default)
    {
        // 1. Obter a nota e verificar existência
        NotaFiscalDto notaParaFechar;
        try
        {
            notaParaFechar = await _faturamentoService.ObterNotaFiscalAsync(numero, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        // 2. Debitar saldo no ServicoEstoque para cada item
        try
        {
            var clienteEstoque = _httpClientFactory.CreateClient("Estoque");

            foreach (var item in notaParaFechar.Itens)
            {
                var debitoPayload = new AlterarSaldoPayload(Quantidade: -item.Quantidade);

                var resposta = await clienteEstoque.PatchAsJsonAsync(
                    $"api/estoque/produtos/{item.CodigoProduto}/saldo",
                    debitoPayload,
                    cancellationToken);

                if (!resposta.IsSuccessStatusCode)
                {
                    var conteudoErro = await resposta.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Falha ao debitar saldo do produto {Codigo}: HTTP {Status} — {Erro}",
                        item.CodigoProduto, (int)resposta.StatusCode, conteudoErro);

                    return BadRequest(new ErroResponse(
                        $"Não foi possível debitar o saldo do produto '{item.CodigoProduto}': {conteudoErro}"));
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Serviço de Estoque indisponível ao fechar a nota {Numero}.", numero);
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErroResponse(
                    "O Serviço de Estoque está temporariamente indisponível. " +
                    "Tente novamente em alguns instantes."));
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout ao comunicar com o Serviço de Estoque para a nota {Numero}.", numero);
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErroResponse("Timeout ao comunicar com o Serviço de Estoque. Tente novamente."));
        }

        // 3. Persistir o fechamento da nota
        try
        {
            var notaFechada = await _faturamentoService.FecharNotaFiscalAsync(numero, cancellationToken);
            return Ok(NotaFiscalResponse.De(notaFechada));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErroResponse(ex.Message));
        }
    }
}

// ── Request Records ────────────────────────────────────────────────────────

/// <summary>Payload para adicionar um item a uma nota fiscal.</summary>
public sealed record AdicionarItemRequest
{
    public required string CodigoProduto { get; init; }
    public required string DescricaoProduto { get; init; }
    public decimal Quantidade { get; init; }
}

/// <summary>Payload interno usado ao debitar saldo no ServicoEstoque.</summary>
internal sealed record AlterarSaldoPayload(decimal Quantidade);

// ── Response Records ───────────────────────────────────────────────────────

/// <summary>Representação pública de uma nota fiscal.</summary>
public sealed record NotaFiscalResponse
{
    public long Numero { get; init; }
    public required string Status { get; init; }
    public IReadOnlyCollection<ItemNotaFiscalResponse> Itens { get; init; } = [];

    internal static NotaFiscalResponse De(NotaFiscalDto dto) => new()
    {
        Numero = dto.Numero,
        Status = dto.Status.ToString(),
        Itens = dto.Itens
            .Select(ItemNotaFiscalResponse.De)
            .ToArray()
    };
}

/// <summary>Representação pública de um item de nota fiscal.</summary>
public sealed record ItemNotaFiscalResponse
{
    public required string CodigoProduto { get; init; }
    public required string DescricaoProduto { get; init; }
    public decimal Quantidade { get; init; }

    internal static ItemNotaFiscalResponse De(ItemNotaFiscalDto dto) => new()
    {
        CodigoProduto = dto.CodigoProduto,
        DescricaoProduto = dto.DescricaoProduto,
        Quantidade = dto.Quantidade
    };
}

/// <summary>Resposta padronizada de erro.</summary>
public sealed record ErroResponse(string Mensagem);
