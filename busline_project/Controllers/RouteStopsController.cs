using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteStopsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RouteStopsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteStop>>> GetAll()
        {
            var routeStops = await _context.RouteStops
                .AsNoTracking()
                .ToListAsync();

            return Ok(routeStops);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RouteStop>> GetById(int id)
        {
            var routeStop = await _context.RouteStops
                .AsNoTracking()
                .FirstOrDefaultAsync(rs => rs.Id == id);

            if (routeStop == null)
            {
                return NotFound();
            }

            return Ok(routeStop);
        }

        [HttpPost]
        public async Task<ActionResult<RouteStop>> Create(RouteStop routeStop)
        {
            _context.RouteStops.Add(routeStop);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = routeStop.Id }, routeStop);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, RouteStop routeStop)
        {
            if (id != routeStop.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(routeStop).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.RouteStops.AnyAsync(rs => rs.Id == id);
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
            var routeStop = await _context.RouteStops.FindAsync(id);
            if (routeStop == null)
            {
                return NotFound();
            }

            _context.RouteStops.Remove(routeStop);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
