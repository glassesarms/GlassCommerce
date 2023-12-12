namespace GlassCommerce.Server.Services.CartService
{
    public interface ICartService
    {
        Task<ServiceResponse<List<CartProductDTO>>> GetCartProductsAsync(List<CartItem> cartItems);
    }
}
