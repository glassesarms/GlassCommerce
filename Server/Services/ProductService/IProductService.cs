namespace GlassCommerce.Server.Services.ProductService
{
    public interface IProductService
    {
        Task<ServiceResponse<List<Product>>> GetProductsAsync();
        Task<ServiceResponse<Product>> GetProductAsync(int productId);
        Task<ServiceResponse<List<Product>>> GetProductsByCategoryAsync(string categoryUrl);
        Task<ServiceResponse<ProductSearchResultDTO>> SearchProductsAsync(string searchText, int page);
        Task<ServiceResponse<List<String>>> GetProductSearchSuggestionsAsync(string searchText);
        Task<ServiceResponse<List<Product>>> GetFeaturedProductsAsync();

    }
}
