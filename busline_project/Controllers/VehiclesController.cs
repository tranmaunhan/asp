using busline_project.Data;
using busline_project.Dtos;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehiclesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetAll()
        {
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .ToListAsync();

            return Ok(vehicles);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.VehicleType)
                .Where(v => v.Id == id)
                .Select(v => new VehicleDetailResponse
                {
                    VehicleId = v.Id,
                    LicensePlate = v.LicensePlate,
                    Brand = v.Brand ?? string.Empty,
                    ManufactureYear = v.ManufactureYear,
                    Status = v.Status.ToString(),
                    VehicleType = new VehicleTypeInfo
                    {
                        VehicleTypeId = v.VehicleType.Id,
                        TypeName = v.VehicleType.TypeName,
                        TotalSeats = v.VehicleType.TotalSeats
                    },
                    Seats = _context.SeatTemplates
                        .Where(st => st.VehicleTypeId == v.VehicleTypeId)
                        .OrderBy(st => st.Deck)
                        .ThenBy(st => st.RowIndex)
                        .ThenBy(st => st.ColIndex)
                        .Select(st => new SeatInfo
                        {
                            SeatTemplateId = st.Id,
                            SeatCode = st.SeatCode,
                            RowIndex = st.RowIndex,
                            ColIndex = st.ColIndex,
                            Deck = st.Deck,
                            SeatType = st.SeatType
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<ActionResult<Vehicle>> Create(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return BadRequest("Id mismatch.");
            }

            _context.Entry(vehicle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Vehicles.AnyAsync(v => v.Id == id);
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
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
