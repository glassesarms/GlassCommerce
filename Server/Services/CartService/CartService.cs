using GlassCommerce.Shared;
using System.Security.Claims;

namespace GlassCommerce.Server.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;

        public CartService(DataContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<ServiceResponse<List<CartProductDTO>>> GetCartProductsAsync(List<CartItem> cartItems)
        {
            var result = new ServiceResponse<List<CartProductDTO>>
            {
                Data = new List<CartProductDTO>()
            };
            
            foreach (var item in cartItems)
            {
                var product = await _context.Products
                    .Where(p => p.Id == item.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    continue;
                }

                var productVariant = await _context.ProductVariants
                    .Where(v => v.ProductId == item.ProductId
                        && v.ProductTypeId == item.ProductTypeId)
                    .Include(v => v.ProductType)
                    .FirstOrDefaultAsync();

                if(productVariant  == null)
                {
                    continue;
                }

                var cartProduct = new CartProductDTO
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    ImageUrl = product.ImageUrl,
                    Price = productVariant.Price,
                    ProductType = productVariant.ProductType.Name,
                    ProductTypeId = productVariant.ProductTypeId,
                    Quantity = item.Quantity
                };

                result.Data.Add(cartProduct);
            }

            return result;
        }

        public async Task<ServiceResponse<List<CartProductDTO>>> StoreCartItems(List<CartItem> cartItems)
        {
            cartItems.ForEach(cartItem => cartItem.UserId = _authService.GetUserId());
            _context.CartItems.AddRange(cartItems);
            await _context.SaveChangesAsync();

            return await GetDbCartProducts();
        }

        public async Task<ServiceResponse<int>> GetCartItemsCount()
        {
            var count = (await _context.CartItems.Where(ci => ci.UserId == _authService.GetUserId()).ToListAsync()).Count;
            return new ServiceResponse<int> { Data = count };
        }

        public async Task<ServiceResponse<List<CartProductDTO>>> GetDbCartProducts(int? userId = null)
        {
            if(userId == null)
            {
                userId = _authService.GetUserId();
            }

            return await GetCartProductsAsync(
                await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync());
        }

        public async Task<ServiceResponse<bool>> AddToCartAsync(CartItem cartItem)
        {
            cartItem.UserId = _authService.GetUserId();

            var sameItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == cartItem.ProductId
                    && ci.ProductTypeId == cartItem.ProductTypeId 
                    && ci.UserId == cartItem.UserId);

            if(sameItem != null)
            {
                sameItem.Quantity += cartItem.Quantity;
            }
            else
            {
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true };
        }

        public async Task<ServiceResponse<bool>> UpdateQuantityAsync(CartItem cartItem)
        {
            var dbCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == cartItem.ProductId
                    && ci.ProductTypeId == cartItem.ProductTypeId
                    && ci.UserId == _authService.GetUserId());
            
            if(dbCartItem == null) 
            {
                return new ServiceResponse<bool> 
                {
                    Data = false,
                    Message = "Cart item does not exist.",
                    Success = false
                };
            }
            
            dbCartItem.Quantity = cartItem.Quantity;
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true };
        }

        public async Task<ServiceResponse<bool>> RemoveItemFromCartAsync(int productId, int productTypeId)
        {
            var dbCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == productId
                    && ci.ProductTypeId == productTypeId
                    && ci.UserId == _authService.GetUserId());

            if (dbCartItem == null)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Message = "Cart item does not exist.",
                    Success = false
                };
            }

            _context.CartItems.Remove(dbCartItem);
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true };
        }
    }
}
