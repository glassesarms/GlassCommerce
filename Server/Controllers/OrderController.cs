using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GlassCommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<bool>>> PlaceOrder()
        {
            var result = await _orderService.PlaceOrderAsync();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<OrderOverviewDTO>>>> GetOrders()
        {
            var result = await _orderService.GetOrderOverviewAsync();
            return Ok(result);
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<ServiceResponse<OrderDetailsDTO>>> GetOrderDetails(int orderId)
        {
            var result = await _orderService.GetOrderDetailsAsync(orderId);
            return Ok(result);
        }
    }
}
