using System.Security.Claims;

namespace GlassCommerce.Server.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly DataContext _context;
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;

        public OrderService(DataContext context, 
            ICartService cartService,
            IAuthService authService)
        {
            _context = context;
            _cartService = cartService;
            _authService = authService;
        }

        public async Task<ServiceResponse<OrderDetailsDTO>> GetOrderDetailsAsync(int orderId)
        {
            var response = new ServiceResponse<OrderDetailsDTO>();
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductType)
                .Where(o => o.UserId == _authService.GetUserId()
                    && o.Id == orderId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                response.Success = false;
                response.Message = "Order not found.";
                return response;
            }

            var orderDetailsDTO = new OrderDetailsDTO
            {
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Products = new List<OrderDetailsProductDTO>()
            };

            order.OrderItems.ForEach(item =>
                orderDetailsDTO.Products.Add( new OrderDetailsProductDTO
                {
                    TotalPrice = item.TotalPrice,
                    ProductId = item.ProductId,
                    ImageUrl = item.Product.ImageUrl,
                    ProductType = item.ProductType.Name,
                    Quantity = item.Quantity,
                    Title = item.Product.Title
                }
            ));

            response.Data = orderDetailsDTO;
            return response;
        }

        public async Task<ServiceResponse<List<OrderOverviewDTO>>> GetOrderOverviewAsync()
        {
            var response = new ServiceResponse<List<OrderOverviewDTO>>();
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == _authService.GetUserId())
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderResponse = new List<OrderOverviewDTO>();
            orders.ForEach(o => orderResponse.Add(
                new OrderOverviewDTO
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Product = o.OrderItems.Count > 1 ?
                        $"{o.OrderItems.First().Product.Title} and" +
                        $" {o.OrderItems.Count - 1} more..." :
                        o.OrderItems.First().Product.Title,
                    ProductImageUrl = o.OrderItems.First().Product.ImageUrl
                }));

            response.Data = orderResponse;
            return response;
        }

        public async Task<ServiceResponse<bool>> PlaceOrderAsync(int userId)
        {
            var products = (await _cartService.GetDbCartProducts(userId)).Data;
            decimal totalPrice = 0;
            products.ForEach(p => totalPrice += (p.Price * p.Quantity));

            var orderItems = new List<OrderItem>();
            products.ForEach(p => orderItems.Add(new OrderItem
            {
                ProductId = p.ProductId,
                ProductTypeId = p.ProductTypeId,
                Quantity = p.Quantity,
                TotalPrice = (p.Price * p.Quantity)
            }));

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalPrice = totalPrice,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);

            _context.CartItems.RemoveRange(_context.CartItems
                .Where(ci => ci.UserId == userId));

            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> 
            {
                Data = true,
                Message = "Your order has been placed, thanks"
            };
        }
    }
}
