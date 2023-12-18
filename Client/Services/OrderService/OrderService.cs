

using Microsoft.AspNetCore.Components;

namespace GlassCommerce.Client.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private readonly IAuthService _authService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;

        public OrderService(HttpClient http,
            IAuthService authService,
            NavigationManager navigationManager)
        {
            _http = http;
            _authService = authService;
            _navigationManager = navigationManager;
        }

        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<OrderDetailsDTO>>($"api/order/{orderId}");
            return result.Data;
        }

        public async Task<List<OrderOverviewDTO>> GetOrders()
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<List<OrderOverviewDTO>>>("api/order");
            return result.Data;
        }

        public async Task<string> PlaceOrder()
        {
            if(await _authService.IsUserAuthenticated())
            {
                var result = await _http.PostAsync("api/payment/checkout", null);
                var url = await result.Content.ReadAsStringAsync();
                return url;
            }
            else
            {
                return "login";
            }
        }
    }
}
