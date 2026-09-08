using System.Security.Claims;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Membera.Merchant.Application.Merchants.UpdateMerchant;
using Membera.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Membera.Merchant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

    private Guid GetOwnerId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub")!);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMerchantRequest request)
    {
        var command = new CreateMerchantCommand(GetOwnerId(), request.BusinessName);
        var result = await _createMerchantHandler.HandleAsync(command);
        return Ok(BaseResponse<CreateMerchantResult>.SuccessResponse(result));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _getMerchantByOwnerIdHandler.HandleAsync(new GetMerchantByOwnerIdQuery(GetOwnerId()));
        return Ok(BaseResponse<GetMerchantByOwnerIdResult>.SuccessResponse(result));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateMerchantRequest request)
    {
        var command = new UpdateMerchantCommand(GetOwnerId(), request.BusinessName, request.Description);
        await _updateMerchantHandler.HandleAsync(command);
        return NoContent();
    }
}

public record CreateMerchantRequest(string BusinessName);
public record UpdateMerchantRequest(string BusinessName, string? Description);