using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetAll()
        {
            var trips = await _context.Trips
                .AsNoTracking()
                .ToListAsync();

            return Ok(trips);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Trip>> GetById(int id)
        {
            var trip = await _context.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null)
            {
                return NotFound();
            }

            return Ok(trip);
        }

        [HttpPost]
        public async Task<ActionResult<Trip>> Create(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = trip.Id }, trip);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Trip trip)
        {
            if (id != trip.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(trip).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Trips.AnyAsync(t => t.Id == id);
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
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
