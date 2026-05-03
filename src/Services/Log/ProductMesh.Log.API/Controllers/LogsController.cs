using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductMesh.Log.Application.DTOs;
using ProductMesh.Log.Application.Interfaces;
using LogLevel = ProductMesh.Log.Domain.Entities.LogLevel;

namespace ProductMesh.Log.API.Controllers;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Query logs with filters — Admin only access for security.
    /// Structured logging with severity levels: INFO, WARNING, ERROR, CRITICAL.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? serviceName,
        [FromQuery] LogLevel? level,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _logService.GetLogsAsync(
            new LogFilterRequest(serviceName, level, from, to, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _logService.GetLogByIdAsync(id);
        return result.Succeeded ? Ok(result) : NotFound(result);
    }
}
