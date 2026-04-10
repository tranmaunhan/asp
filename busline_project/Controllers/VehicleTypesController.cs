using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehicleTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleType>>> GetAll()
        {
            var vehicleTypes = await _context.VehicleTypes
                .AsNoTracking()
                .ToListAsync();

            return Ok(vehicleTypes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VehicleType>> GetById(int id)
        {
            var vehicleType = await _context.VehicleTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(vt => vt.Id == id);

            if (vehicleType == null)
            {
                return NotFound();
            }

            return Ok(vehicleType);
        }

        [HttpPost]
        public async Task<ActionResult<VehicleType>> Create(VehicleType vehicleType)
        {
            _context.VehicleTypes.Add(vehicleType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vehicleType.Id }, vehicleType);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, VehicleType vehicleType)
        {
            if (id != vehicleType.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(vehicleType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.VehicleTypes.AnyAsync(vt => vt.Id == id);
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
            var vehicleType = await _context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                return NotFound();
            }

            _context.VehicleTypes.Remove(vehicleType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
