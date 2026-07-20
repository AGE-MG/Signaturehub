using System.Security.Claims;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.DTOs.Notifications;
using AGE.SignatureHub.Infrastructure.Services.Webhook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/external-services")]
public sealed class ExternalServicesController : ControllerBase
{
    private readonly IExternalServiceConnectionService _service;
    public ExternalServicesController(IExternalServiceConnectionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(new { success = true, data = await _service.GetAsync(UserId(), cancellationToken), allowedEvents = ExternalServiceConnectionService.AllowedEvents });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveExternalServiceConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(UserId(), request, cancellationToken);
        return Created($"api/v1/external-services/{result.Id}", new { success = true, data = result, message = "Integração criada. Copie o segredo agora; ele não será exibido novamente." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveExternalServiceConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(UserId(), id, request, cancellationToken);
        return result is null ? NotFound() : Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetExternalServiceActiveRequest request, CancellationToken cancellationToken) =>
        await _service.SetActiveAsync(UserId(), id, request.Active, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await _service.DeleteAsync(UserId(), id, cancellationToken) ? NoContent() : NotFound();

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed class SetExternalServiceActiveRequest { public bool Active { get; init; } }
