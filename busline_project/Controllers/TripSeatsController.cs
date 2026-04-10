using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TripSeatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripSeatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripSeat>>> GetAll()
        {
            var tripSeats = await _context.TripSeats
                .AsNoTracking()
                .ToListAsync();

            return Ok(tripSeats);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TripSeat>> GetById(int id)
        {
            var tripSeat = await _context.TripSeats
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (tripSeat == null)
            {
                return NotFound();
            }

            return Ok(tripSeat);
        }

        [HttpPost]
        public async Task<ActionResult<TripSeat>> Create(TripSeat tripSeat)
        {
            _context.TripSeats.Add(tripSeat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tripSeat.Id }, tripSeat);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, TripSeat tripSeat)
        {
            if (id != tripSeat.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(tripSeat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.TripSeats.AnyAsync(ts => ts.Id == id);
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
            var tripSeat = await _context.TripSeats.FindAsync(id);
            if (tripSeat == null)
            {
                return NotFound();
            }

            _context.TripSeats.Remove(tripSeat);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
