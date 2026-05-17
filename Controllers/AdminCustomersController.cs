using EcommerceApi.DTOs;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers
{
    [Route("api/Admin/customers")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCustomersController : ControllerBase
    {
        private readonly CustomerRankingService _ranking;

        public AdminCustomersController(CustomerRankingService ranking)
        {
            _ranking = ranking;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerRankDto>>> GetRankedCustomers()
        {
            return Ok(await _ranking.GetRankedCustomersAsync());
        }
    }
}
