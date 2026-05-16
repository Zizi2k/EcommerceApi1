using EcommerceApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var list = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();

            return Ok(list);
        }
    }
}
