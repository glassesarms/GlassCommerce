namespace GlassCommerce.Server.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<ServiceResponse<List<Category>>> GetCategoriesAsync();
    }
}
