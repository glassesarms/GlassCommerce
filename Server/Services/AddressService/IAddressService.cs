namespace GlassCommerce.Server.Services.AddressService
{
    public interface IAddressService
    {
        Task<ServiceResponse<Address>> GetAddressAsync();
        Task<ServiceResponse<Address>> AddOrUpdateAddressAsync(Address address);
    }
}
