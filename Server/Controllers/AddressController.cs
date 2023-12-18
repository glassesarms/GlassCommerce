using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GlassCommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<Address>>> GetAddress()
        {
            var response = await _addressService.GetAddressAsync();
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<Address>>> AddOrUpdateAddress(Address address)
        {
            var response = await _addressService.AddOrUpdateAddressAsync(address);
            return Ok(response);
        }
    }
}
