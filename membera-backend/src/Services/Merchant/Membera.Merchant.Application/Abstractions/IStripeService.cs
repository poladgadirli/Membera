namespace Membera.Merchant.Application.Abstractions;

public interface IStripeService
{
    Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
        string planName, decimal price, string successUrl, string cancelUrl);
}
