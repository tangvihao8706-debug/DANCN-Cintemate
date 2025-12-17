using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Microsoft.AspNetCore.Identity;
namespace EventManager.Controllers
{
    public class PaymentController : Controller
    {
        public async Task<IActionResult> Checkout(decimal amount)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Sự kiện online"
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = "https://localhost:5001/Payment/Success",
                CancelUrl = "https://localhost:5001/Payment/Cancel"
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return Redirect(session.Url);
        }

    }
}
