using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetAll()
        {
            var locations = await _context.Locations
                .AsNoTracking()
                .ToListAsync();

            return Ok(locations);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Location>> GetById(int id)
        {
            var location = await _context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<Location>> Create(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Locations.AnyAsync(l => l.Id == id);
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
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpGet("pickup-options")]
        public async Task<ActionResult<IEnumerable<Location>>> GetPickupOptions()
        {
            var locations = await _context.Locations
                .AsNoTracking()
                .Where(l => l.Type != LocationType.HIGHWAY)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return Ok(locations);
        }
    }
}
