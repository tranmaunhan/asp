using busline_project.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteEntity = busline_project.Models.Route;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteEntity>>> GetAll()
        {
            var routes = await _context.Routes
                .AsNoTracking()
                .ToListAsync();

            return Ok(routes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RouteEntity>> GetById(int id)
        {
            var route = await _context.Routes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
            {
                return NotFound();
            }

            return Ok(route);
        }

        [HttpPost]
        public async Task<ActionResult<RouteEntity>> Create(RouteEntity route)
        {
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, RouteEntity route)
        {
            if (id != route.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(route).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Routes.AnyAsync(r => r.Id == id);
                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
