
namespace GlassCommerce.Server.Services.AddressService
{
    public class AddressService : IAddressService
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;

        public AddressService(DataContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<ServiceResponse<Address>> AddOrUpdateAddressAsync(Address address)
        {
            var response = new ServiceResponse<Address>();
            var currentAddress = (await GetAddressAsync()).Data;
            if( currentAddress == null )
            {
                address.UserId = _authService.GetUserId();
                _context.Addresses.Add(address);
                response.Data = address;
            }
            else
            {
                currentAddress.FirstName = address.FirstName;
                currentAddress.LastName = address.LastName;
                currentAddress.Code = address.Code;
                currentAddress.Street = address.Street;
                currentAddress.City = address.City;
                currentAddress.Country = address.Country;
                currentAddress.State = address.State;
                response.Data = currentAddress;
            }

            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<ServiceResponse<Address>> GetAddressAsync()
        {
            int userId = _authService.GetUserId();
            var address = await _context.Addresses.FirstOrDefaultAsync(
                a => a.UserId == userId);

            return new ServiceResponse<Address>
            {
                Data = address
            };
        }
    }
}
