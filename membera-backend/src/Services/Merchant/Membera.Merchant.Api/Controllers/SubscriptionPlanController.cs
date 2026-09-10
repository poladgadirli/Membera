using System.Security.Claims;
using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.CreateSubscriptionPlan;
using Membera.Merchant.Application.SubscriptionPlans.DeactivateSubscriptionPlan;
using Membera.Merchant.Application.SubscriptionPlans.GetPlansByMerchantId;
using Membera.Merchant.Application.SubscriptionPlans.UpdateSubscriptionPlan;
using Membera.Merchant.Application.SubscriptionPlans.UploadSubscriptionPlanImage;
using Membera.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Membera.Merchant.Api.Controllers;

[ApiController]
[Route("api/subscription-plans")]
[Authorize]
public class SubscriptionPlanController : ControllerBase
{
    private readonly CreateSubscriptionPlanHandler _createHandler;
    private readonly GetPlansByMerchantIdHandler _getByMerchantIdHandler;
    private readonly UpdateSubscriptionPlanHandler _updateHandler;
    private readonly DeactivateSubscriptionPlanHandler _deactivateHandler;
    private readonly UploadSubscriptionPlanImageHandler _uploadImageHandler;
    private readonly IMerchantRepository _merchantRepository;

    public SubscriptionPlanController(
        CreateSubscriptionPlanHandler createHandler,
        GetPlansByMerchantIdHandler getByMerchantIdHandler,
        UpdateSubscriptionPlanHandler updateHandler,
        DeactivateSubscriptionPlanHandler deactivateHandler,
        UploadSubscriptionPlanImageHandler uploadImageHandler,
        IMerchantRepository merchantRepository)
    {
        _createHandler = createHandler;
        _getByMerchantIdHandler = getByMerchantIdHandler;
        _updateHandler = updateHandler;
        _deactivateHandler = deactivateHandler;
        _uploadImageHandler = uploadImageHandler;
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
    public async Task<IActionResult> Create(CreateSubscriptionPlanRequest request)
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();

        var command = new CreateSubscriptionPlanCommand(
            merchantId, request.Name, request.Description, request.Price,
            request.DurationInDays, request.UsageLimit, request.ActiveFrom, request.ActiveUntil);

        var result = await _createHandler.HandleAsync(command);
        return Ok(BaseResponse<CreateSubscriptionPlanResult>.SuccessResponse(result));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();
        var result = await _getByMerchantIdHandler.HandleAsync(new GetPlansByMerchantIdQuery(merchantId));
        return Ok(BaseResponse<GetPlansByMerchantIdResult>.SuccessResponse(result));
    }

    [HttpPut("{planId}")]
    public async Task<IActionResult> Update(Guid planId, UpdateSubscriptionPlanRequest request)
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();

        var command = new UpdateSubscriptionPlanCommand(
            planId, merchantId, request.Name, request.Description, request.Price,
            request.DurationInDays, request.UsageLimit, request.ActiveFrom, request.ActiveUntil);

        await _updateHandler.HandleAsync(command);
        return NoContent();
    }

    [HttpPost("{planId}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid planId)
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();
        await _deactivateHandler.HandleAsync(new DeactivateSubscriptionPlanCommand(planId, merchantId));
        return NoContent();
    }

    [HttpPost("{planId}/image")]
    public async Task<IActionResult> UploadImage(Guid planId, [FromForm] IFormFile file)
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();

        await using var stream = file.OpenReadStream();
        var command = new UploadSubscriptionPlanImageCommand(
            planId, merchantId, stream, file.FileName, file.ContentType);

        var imageUrl = await _uploadImageHandler.HandleAsync(command);
        return Ok(BaseResponse<string>.SuccessResponse(imageUrl));
    }
}

public record CreateSubscriptionPlanRequest(
    string Name, string? Description, decimal Price, int DurationInDays,
    int? UsageLimit, TimeOnly ActiveFrom, TimeOnly ActiveUntil);

public record UpdateSubscriptionPlanRequest(
    string Name, string? Description, decimal Price, int DurationInDays,
    int? UsageLimit, TimeOnly ActiveFrom, TimeOnly ActiveUntil);