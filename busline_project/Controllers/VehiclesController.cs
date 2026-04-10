using busline_project.Data;
using busline_project.Dtos;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            var data = await _context.Set<VehicleSeatDto>()
                .FromSqlRaw("EXEC GetVehicleWithSeats @VehicleId",
                    new SqlParameter("@VehicleId", id))
                .ToListAsync();

            if (data == null || data.Count == 0)
            {
                return NotFound();
            }

            var first = data[0];
            var result = new VehicleDetailResponse
            {
                VehicleId = first.VehicleId,
                LicensePlate = first.LicensePlate,
                Brand = first.Brand,
                ManufactureYear = first.ManufactureYear,
                Status = first.Status,
                VehicleType = new VehicleTypeInfo
                {
                    VehicleTypeId = first.VehicleTypeId,
                    TypeName = first.TypeName,
                    TotalSeats = first.TotalSeats
                },
                Seats = data.Select(s => new SeatInfo
                {
                    SeatTemplateId = s.SeatTemplateId,
                    SeatCode = s.SeatCode,
                    RowIndex = s.RowIndex,
                    ColIndex = s.ColIndex,
                    Deck = s.Deck,
                    SeatType = s.SeatType
                }).ToList()
            };

            return Ok(result);
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
