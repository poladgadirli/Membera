using System.Security.Claims;
using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Membera.Merchant.Application.Merchants.UpdateMerchant;
using Membera.Merchant.Application.Merchants.UploadMerchantLogo;
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
    private readonly UploadMerchantLogoHandler _uploadMerchantLogoHandler;
    private readonly IMerchantRepository _merchantRepository;

    public MerchantController(
        CreateMerchantHandler createMerchantHandler,
        GetMerchantByOwnerIdHandler getMerchantByOwnerIdHandler,
        UpdateMerchantHandler updateMerchantHandler,
        UploadMerchantLogoHandler uploadMerchantLogoHandler,
        IMerchantRepository merchantRepository)
    {
        _createMerchantHandler = createMerchantHandler;
        _getMerchantByOwnerIdHandler = getMerchantByOwnerIdHandler;
        _updateMerchantHandler = updateMerchantHandler;
        _uploadMerchantLogoHandler = uploadMerchantLogoHandler;
        _merchantRepository = merchantRepository;
    }

    private Guid GetOwnerId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub")!);
    }

    private async Task<Guid> GetMerchantIdForCurrentUserAsync()
    {
        var merchant = await _merchantRepository.GetByOwnerIdAsync(GetOwnerId());
        if (merchant is null)
            throw new InvalidOperationException("You don't have a merchant profile.");

        return merchant.Id;
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

    [HttpPost("logo")]
    // No [FromForm]: an IFormFile parameter is already bound from multipart form
    // data by [ApiController] inference, and Swashbuckle throws at swagger-gen
    // time if [FromForm] is combined with IFormFile (by design — see its
    // "Handle Forms and File Uploads" docs).
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();

        await using var stream = file.OpenReadStream();
        var command = new UploadMerchantLogoCommand(merchantId, stream, file.FileName, file.ContentType);

        var logoUrl = await _uploadMerchantLogoHandler.HandleAsync(command);
        return Ok(BaseResponse<string>.SuccessResponse(logoUrl));
    }
}

public record CreateMerchantRequest(string BusinessName);
public record UpdateMerchantRequest(string BusinessName, string? Description);