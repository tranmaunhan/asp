using busline_project.Data;
using busline_project.Dtos;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /trips/fullroutes?date=2026-05-01  (date optional)
        [HttpGet("fullroutes")]
        public async Task<IActionResult> GetAllFullRoutes([FromQuery] DateTime? date)
        {
            var results = new List<Dictionary<string, object?>>();

            var conn = _context.Database.GetDbConnection();
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT * FROM get_all_trips_full_route(@p_date);", (NpgsqlConnection)conn);

                if (date.HasValue)
                {
                    cmd.Parameters.AddWithValue("p_date", NpgsqlTypes.NpgsqlDbType.Date, date.Value.Date);
                }
                else
                {
                    // pass NULL to the function
                    cmd.Parameters.AddWithValue("p_date", DBNull.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                        // Normalize date/time coming from DB to avoid timezone/Kind surprises.
                        if (value is DateTime dt)
                        {
                            // If DB returned an unspecified kind (common with timestamptz), treat as UTC.
                            if (dt.Kind == DateTimeKind.Unspecified)
                                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                            value = dt;
                        }
                        else if (value is DateTimeOffset dto)
                        {
                            // Convert DateTimeOffset to UTC DateTime for consistent serialization
                            value = dto.UtcDateTime;
                        }

                        row[name] = value;
                    }
                    results.Add(row);
                }

                return Ok(results);
            }
            catch (DbException dbEx)
            {
                return StatusCode(500, new { message = "Database error", detail = dbEx.Message });
            }
            finally
            {
                try { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); } catch { }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFull()
        {
            var trips = await _context.Trips
                .AsNoTracking()
                .Include(t => t.Route)
                .Include(t => t.Vehicle)
                    .ThenInclude(v => v.VehicleType)
                .Include(t => t.TripSeats)
                .Select(t => new
                {
                    t.Id,
                    t.DepartureTime,
                    t.Status,

                    Route = new
                    {
                        t.Route.Id,
                        t.Route.OriginId,
                        t.Route.DestinationId,
                        t.Route.DistanceKm,
                        t.Route.EstimatedDurationMinutes
                    },

                    Vehicle = new
                    {
                        t.Vehicle.Id,
                        t.Vehicle.LicensePlate,
                        VehicleType = new
                        {
                            t.Vehicle.VehicleType.Id,
                            t.Vehicle.VehicleType.TypeName,
                            t.Vehicle.VehicleType.TotalSeats
                        }
                    },

                    TotalSeats = t.TripSeats.Count,
                    AvailableSeats = t.TripSeats.Count(s => s.Status == TripSeatStatus.Available),
                    BookedSeats = t.TripSeats.Count(s => s.Status == TripSeatStatus.Booked)
                })
                .ToListAsync();

            return Ok(trips);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTripDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Check Route
                var routeExists = await _context.Routes
                    .AnyAsync(r => r.Id == dto.RouteId);

                if (!routeExists)
                    return BadRequest("Route không tồn tại");

                // 2. Lấy Vehicle
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

                if (vehicle == null)
                    return BadRequest("Vehicle không tồn tại");

                // 3. Lấy SeatTemplates theo VehicleType
                var seatTemplates = await _context.SeatTemplates
                    .Where(st => st.VehicleTypeId == vehicle.VehicleTypeId)
                    .ToListAsync();

                if (!seatTemplates.Any())
                    return BadRequest("Xe này chưa có sơ đồ ghế");

                // 4. Map DTO → Entity Trip
                var trip = new Trip
                {
                    RouteId = dto.RouteId,
                    VehicleId = dto.VehicleId,
                    DepartureTime = dto.DepartureTime,
                    Status = dto.Status
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                // 5. Tạo TripSeats từ sơ đồ ghế
                var tripSeats = seatTemplates.Select(st => new TripSeat
                {
                    TripId = trip.Id,
                    SeatTemplateId = st.Id,
                    Status = TripSeatStatus.Available
                }).ToList();

                _context.TripSeats.AddRange(tripSeats);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Tạo chuyến thành công",
                    tripId = trip.Id,
                    totalSeats = tripSeats.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateTripDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var trip = await _context.Trips.FindAsync(id);
                if (trip == null)
                    return NotFound("Không tìm thấy chuyến");

                var hasTickets = await _context.Tickets
                    .AnyAsync(t => t.TripId == id);

                if (dto.DepartureTime < DateTime.UtcNow.AddMinutes(-5))
                {
                    return BadRequest("Thời gian khởi hành không hợp lệ");
                }

                if (hasTickets)
                {
                    trip.Status = dto.Status;
                }
                else
                {
                    trip.DepartureTime = dto.DepartureTime;
                    trip.Status = dto.Status;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Cập nhật chuyến thành công",
                    tripId = trip.Id,
                    status = trip.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return NotFound();

            var hasTickets = await _context.Tickets
                .AnyAsync(t => t.TripId == id);

            if (hasTickets)
                return BadRequest("Trip đã có vé, không thể xóa");

            var seats = _context.TripSeats.Where(ts => ts.TripId == id);
            _context.TripSeats.RemoveRange(seats);

            _context.Trips.Remove(trip);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET /trips/fullroute?originId=3&destinationId=4&date=2026-05-01
        [HttpGet("fullroute")]
        public async Task<IActionResult> GetFullRoute([FromQuery] int originId, [FromQuery] int destinationId, [FromQuery] DateTime date)
        {
            var results = new List<Dictionary<string, object?>>();

            var conn = _context.Database.GetDbConnection();
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT * FROM find_trips_full_route(@originId, @destinationId, @p_date);", (NpgsqlConnection)conn);

                cmd.Parameters.AddWithValue("originId", NpgsqlTypes.NpgsqlDbType.Integer, originId);
                cmd.Parameters.AddWithValue("destinationId", NpgsqlTypes.NpgsqlDbType.Integer, destinationId);
                cmd.Parameters.AddWithValue("p_date", NpgsqlTypes.NpgsqlDbType.Date, date.Date);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);

                        // Normalize date/time coming from DB to avoid timezone/Kind surprises.
                        if (value is DateTime dt)
                        {
                            if (dt.Kind == DateTimeKind.Unspecified)
                                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                            // Convert to server local time for display consistency with psql/UI
                            value = dt.ToLocalTime();
                        }
                        else if (value is DateTimeOffset dto)
                        {
                            value = dto.ToLocalTime().DateTime;
                        }

                        row[name] = value;
                    }
                    results.Add(row);
                }

                return Ok(results);
            }
            catch (DbException dbEx)
            {
                return StatusCode(500, new { message = "Database error", detail = dbEx.Message });
            }
            finally
            {
                try { if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync(); } catch { }
            }
        }


    }
}
