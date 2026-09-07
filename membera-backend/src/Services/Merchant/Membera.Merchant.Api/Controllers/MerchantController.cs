using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Membera.Merchant.Application.Merchants.UpdateMerchant;
using Microsoft.AspNetCore.Mvc;

namespace Membera.Merchant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantController : ControllerBase
{
    private readonly CreateMerchantHandler _createMerchantHandler;
    private readonly GetMerchantByOwnerIdHandler _getMerchantByOwnerIdHandler;
    private readonly UpdateMerchantHandler _updateMerchantHandler;

    public MerchantController(
        CreateMerchantHandler createMerchantHandler,
        GetMerchantByOwnerIdHandler getMerchantByOwnerIdHandler,
        UpdateMerchantHandler updateMerchantHandler)
    {
        _createMerchantHandler = createMerchantHandler;
        _getMerchantByOwnerIdHandler = getMerchantByOwnerIdHandler;
        _updateMerchantHandler = updateMerchantHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMerchantCommand command)
    {
        var result = await _createMerchantHandler.HandleAsync(command);
        return Ok(result);
    }

    [HttpGet("{ownerId}")]
    public async Task<IActionResult> GetByOwnerId(Guid ownerId)
    {
        var result = await _getMerchantByOwnerIdHandler.HandleAsync(new GetMerchantByOwnerIdQuery(ownerId));
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateMerchantCommand command)
    {
        await _updateMerchantHandler.HandleAsync(command);
        return NoContent();
    }
}