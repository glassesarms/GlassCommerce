namespace GlassCommerce.Server.Services.OrderService
{
    public interface IOrderService
    {
        Task<ServiceResponse<bool>> PlaceOrderAsync();
        Task<ServiceResponse<List<OrderOverviewDTO>>> GetOrderOverviewAsync();
        Task<ServiceResponse<OrderDetailsDTO>> GetOrderDetailsAsync(int orderId);

    }
}
