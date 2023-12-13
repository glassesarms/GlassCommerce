﻿using System.Security.Claims;

namespace GlassCommerce.Server.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

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
            cartItems.ForEach(cartItem => cartItem.UserId = GetUserId());
            _context.CartItems.AddRange(cartItems);
            await _context.SaveChangesAsync();

            return await GetDbCartProducts();
        }

        public async Task<ServiceResponse<int>> GetCartItemsCount()
        {
            var count = (await _context.CartItems.Where(ci => ci.UserId == GetUserId()).ToListAsync()).Count;
            return new ServiceResponse<int> { Data = count };
        }

        public async Task<ServiceResponse<List<CartProductDTO>>> GetDbCartProducts()
        {
            return await GetCartProductsAsync(
                await _context.CartItems
                .Where(ci => ci.UserId == GetUserId())
                .ToListAsync());
        }

        public async Task<ServiceResponse<bool>> AddToCartAsync(CartItem cartItem)
        {
            cartItem.UserId = GetUserId();

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
    }
}
