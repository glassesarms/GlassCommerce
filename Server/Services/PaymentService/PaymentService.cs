using Stripe;
using Stripe.Checkout;

namespace GlassCommerce.Server.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;

        const string secret = "whsec_7456c601360610b03636f17c0223d93a9940bbe6c90550d65b28a74c07a388d6";

        public PaymentService(ICartService cartService,
            IAuthService authService,
            IOrderService orderService)
        {
            StripeConfiguration.ApiKey = "sk_test_51ON482CzZrRR3FwjfMDL4zG8zc6gUxDKa7J45XV9U5kgsDxdKGLVBMaEH7a7EFDnNvUJ64JbOJc2RFtvgi0sJX1800B4fdTa7r";

            _cartService = cartService;
            _authService = authService;
            _orderService = orderService;
        }

        public async Task<Session> CreateCheckoutSessionAsync()
        {
            var products = (await _cartService.GetDbCartProducts()).Data;
            var lineItems = new List<SessionLineItemOptions>();
            products.ForEach(p => lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = p.Price * 100,
                    Currency = "aud",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = p.Title,
                        Images = new List<string>
                        {
                            p.ImageUrl
                        }
                    }
                },
                Quantity = p.Quantity
            }));

            var options = new SessionCreateOptions
            {
                CustomerEmail = _authService.GetUserEmail(),
                ShippingAddressCollection = 
                    new SessionShippingAddressCollectionOptions
                    {
                        AllowedCountries = new List<string> { "AU" }
                    },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = "https://localhost:7232/order-success",
                CancelUrl = "https://localhost:7232/cart"
            };

            var service = new SessionService();
            Session session = service.Create(options);
            return session;
        }

        public async Task<ServiceResponse<bool>> FulfillOrderAsync(HttpRequest request)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                        json,
                        request.Headers["Stripe-Signature"],
                        secret
                    );

                if(stripeEvent.Type == Events.CheckoutSessionCompleted )
                {
                    var session = stripeEvent.Data.Object as Session;
                    var user = await _authService.GetUserByEmailAsync(session.CustomerEmail);
                    await _orderService.PlaceOrderAsync(user.Id);
                }

                return new ServiceResponse<bool> { Data = true };

            } catch(StripeException e)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = e.Message
                };
            }
        }
    }
}
