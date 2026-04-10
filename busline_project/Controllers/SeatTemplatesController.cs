using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeatTemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeatTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeatTemplate>>> GetAll()
        {
            var seatTemplates = await _context.SeatTemplates
                .AsNoTracking()
                .ToListAsync();

            return Ok(seatTemplates);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SeatTemplate>> GetById(int id)
        {
            var seatTemplate = await _context.SeatTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(st => st.Id == id);

            if (seatTemplate == null)
            {
                return NotFound();
            }

            return Ok(seatTemplate);
        }

        [HttpPost]
        public async Task<ActionResult<SeatTemplate>> Create(SeatTemplate seatTemplate)
        {
            _context.SeatTemplates.Add(seatTemplate);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = seatTemplate.Id }, seatTemplate);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, SeatTemplate seatTemplate)
        {
            if (id != seatTemplate.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(seatTemplate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.SeatTemplates.AnyAsync(st => st.Id == id);
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
            var seatTemplate = await _context.SeatTemplates.FindAsync(id);
            if (seatTemplate == null)
            {
                return NotFound();
            }

            _context.SeatTemplates.Remove(seatTemplate);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
