using System.Security.Claims;
using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Subscriptions.CreateCheckoutSession;
using Membera.Merchant.Application.Subscriptions.GetMySubscriptions;
using Membera.Merchant.Application.Subscriptions.HandleStripeWebhook;
using Membera.Merchant.Application.Subscriptions.RedeemSubscription;
using Membera.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Membera.Merchant.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly CreateCheckoutSessionHandler _createCheckoutSessionHandler;
    private readonly HandleStripeWebhookHandler _handleStripeWebhookHandler;
    private readonly RedeemSubscriptionHandler _redeemSubscriptionHandler;
    private readonly GetMySubscriptionsHandler _getMySubscriptionsHandler;
    private readonly IMerchantRepository _merchantRepository;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        CreateCheckoutSessionHandler createCheckoutSessionHandler,
        HandleStripeWebhookHandler handleStripeWebhookHandler,
        RedeemSubscriptionHandler redeemSubscriptionHandler,
        GetMySubscriptionsHandler getMySubscriptionsHandler,
        IMerchantRepository merchantRepository,
        ILogger<SubscriptionController> logger)
    {
        _createCheckoutSessionHandler = createCheckoutSessionHandler;
        _handleStripeWebhookHandler = handleStripeWebhookHandler;
        _redeemSubscriptionHandler = redeemSubscriptionHandler;
        _getMySubscriptionsHandler = getMySubscriptionsHandler;
        _merchantRepository = merchantRepository;
        _logger = logger;
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

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        var userId = GetOwnerId();

        // Placeholder URLs. These should eventually point to the frontend so the
        // customer lands on a proper confirmation / cancellation page.
        const string successUrl = "https://localhost:7241/api/subscriptions/success";
        const string cancelUrl = "https://localhost:7241/api/subscriptions/cancel";

        var command = new CreateCheckoutSessionCommand(
            userId, request.SubscriptionPlanId, successUrl, cancelUrl);

        var result = await _createCheckoutSessionHandler.HandleAsync(command);
        return Ok(BaseResponse<CreateCheckoutSessionResult>.SuccessResponse(result));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        // Stripe calls this endpoint directly, so it is not [Authorize]d.
        // SIMPLIFIED: no signature verification yet. In production this MUST verify
        // the "Stripe-Signature" header against a configured webhook signing secret
        // (EventUtility.ConstructEvent) to reject forged payloads.
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session is null)
                {
                    _logger.LogWarning("Stripe webhook 'checkout.session.completed' had no session payload.");
                    return BadRequest();
                }

                await _handleStripeWebhookHandler.HandleAsync(new HandleStripeWebhookCommand(session.Id));
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // Never return 500 to Stripe for a payload we could not process.
            _logger.LogError(ex, "Failed to process Stripe webhook payload.");
            return BadRequest();
        }
    }

    [Authorize]
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem(RedeemRequest request)
    {
        var merchantId = await GetMerchantIdForCurrentUserAsync();

        var command = new RedeemSubscriptionCommand(request.RedemptionCode, merchantId);
        var result = await _redeemSubscriptionHandler.HandleAsync(command);
        return Ok(BaseResponse<RedeemSubscriptionResult>.SuccessResponse(result));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetOwnerId();
        var result = await _getMySubscriptionsHandler.HandleAsync(new GetMySubscriptionsQuery(userId));
        return Ok(BaseResponse<GetMySubscriptionsResult>.SuccessResponse(result));
    }
}

public record CheckoutRequest(Guid SubscriptionPlanId);

public record RedeemRequest(string RedemptionCode);
