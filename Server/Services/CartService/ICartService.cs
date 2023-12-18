namespace GlassCommerce.Server.Services.CartService
{
    public interface ICartService
    {
        Task<ServiceResponse<List<CartProductDTO>>> GetCartProductsAsync(List<CartItem> cartItems);
        Task<ServiceResponse<List<CartProductDTO>>> StoreCartItems(List<CartItem> cartItems);
        Task<ServiceResponse<int>> GetCartItemsCount();
        Task<ServiceResponse<List<CartProductDTO>>> GetDbCartProducts(int? userId = null);
        Task<ServiceResponse<bool>> AddToCartAsync(CartItem cartItem);
        Task<ServiceResponse<bool>> UpdateQuantityAsync(CartItem cartItem);
        Task<ServiceResponse<bool>> RemoveItemFromCartAsync(int productId, int productTypeId);
    }
}
