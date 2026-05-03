using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductMesh.Product.Application.Commands;
using ProductMesh.Product.Application.DTOs;
using ProductMesh.Product.Application.Queries;

namespace ProductMesh.Product.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products (public, cached via Redis).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(page, pageSize));
        return result.Succeeded ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get product by ID (public, cached via Redis).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return result.Succeeded ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create product — requires JWT authentication.
    /// Uses CQRS Command Handler for async database write.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var command = new CreateProductCommand(
            request.Name, request.Description, request.Price,
            request.Stock, request.Category, userId);

        var result = await _mediator.Send(command);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update product — requires JWT authentication as per requirements.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var command = new UpdateProductCommand(
            id, request.Name, request.Description, request.Price,
            request.Stock, request.Category, userId);

        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
