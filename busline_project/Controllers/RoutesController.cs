using busline_project.Data;
using busline_project.Dtos;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteEntity = busline_project.Models.Route;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteViewModel>>> GetAll()
        {
            var routes = await _context
                .Set<RouteViewModel>()
                .FromSqlRaw("SELECT * FROM get_routes_with_details()")
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
        public async Task<ActionResult> Create(RouteEntity route)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validate origin != destination
                if (route.OriginId == route.DestinationId)
                {
                    return BadRequest("Origin and Destination cannot be the same");
                }

                // 2. Check location tồn tại
                var originExists = await _context.Locations.AnyAsync(x => x.Id == route.OriginId);
                var destinationExists = await _context.Locations.AnyAsync(x => x.Id == route.DestinationId);

                if (!originExists || !destinationExists)
                {
                    return BadRequest("Origin or Destination does not exist");
                }

                // 3. Tạo route mới (không dùng trực tiếp object client gửi)
                var newRoute = new RouteEntity
                {
                    OriginId = route.OriginId,
                    DestinationId = route.DestinationId,
                    DistanceKm = route.DistanceKm,
                    EstimatedDurationMinutes = route.EstimatedDurationMinutes
                };

                _context.Routes.Add(newRoute);
                await _context.SaveChangesAsync();

                // 4. Xử lý RouteStops
                if (route.RouteStops != null && route.RouteStops.Count > 0)
                {
                    var stops = route.RouteStops
                        .OrderBy(x => x.StopOrder)
                        .ToList();

                    int expectedOrder = 1;
                    int lastDistance = -1;

                    foreach (var stop in stops)
                    {
                       
                        if (stop.StopOrder != expectedOrder)
                        {
                            return BadRequest("StopOrder must be continuous starting from 1");
                        }

                        expectedOrder++;

                        var locationExists = await _context.Locations.AnyAsync(x => x.Id == stop.LocationId);
                        if (!locationExists)
                        {
                            return BadRequest($"LocationId {stop.LocationId} does not exist");
                        }

                      
                        if (stop.DistanceFromStartKm.HasValue)
                        {
                            if (stop.DistanceFromStartKm < lastDistance)
                            {
                                return BadRequest("DistanceFromStartKm must increase");
                            }

                            lastDistance = stop.DistanceFromStartKm.Value;
                        }

                        var newStop = new RouteStop
                        {
                            RouteId = newRoute.Id,
                            LocationId = stop.LocationId,
                            StopOrder = stop.StopOrder,
                            DistanceFromStartKm = stop.DistanceFromStartKm,
                            EstimatedTimeFromStartMinutes = stop.EstimatedTimeFromStartMinutes
                        };

                        _context.RouteStops.Add(newStop);
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = newRoute.Id }, newRoute);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RouteUpdateDto request)
        {
            var route = await _context.Routes
                .Include(r => r.RouteStops)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
                return NotFound();

            // ✅ Validate cơ bản
            if (request.RouteStops == null || request.RouteStops.Count == 0)
                return BadRequest("RouteStops không được rỗng");

            // ✅ Validate điểm đầu/cuối
            if (request.RouteStops.First().LocationId != request.OriginId)
                return BadRequest("Stop đầu phải là Origin");

            if (request.RouteStops.Last().LocationId != request.DestinationId)
                return BadRequest("Stop cuối phải là Destination");

            // ✅ Validate StopOrder tăng dần
            var isValidOrder = request.RouteStops
                .OrderBy(s => s.StopOrder)
                .Select((s, i) => s.StopOrder == i + 1)
                .All(x => x);

            if (!isValidOrder)
                return BadRequest("StopOrder phải liên tục từ 1 → n");

            // 🔥 Update Route
            route.OriginId = request.OriginId;
            route.DestinationId = request.DestinationId;
            route.DistanceKm = request.DistanceKm;
            route.EstimatedDurationMinutes = request.EstimatedDurationMinutes;

            // 🔥 Xóa stops cũ
            _context.RouteStops.RemoveRange(route.RouteStops);

            // 🔥 Add stops mới (đã có distance + time)
            var newStops = request.RouteStops.Select(s => new RouteStop
            {
                RouteId = route.Id,
                LocationId = s.LocationId,
                StopOrder = s.StopOrder,
                DistanceFromStartKm = s.DistanceFromStartKm,
                EstimatedTimeFromStartMinutes = s.EstimatedTimeFromStartMinutes
            });

            await _context.RouteStops.AddRangeAsync(newStops);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật tuyến thành công"
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var route = await _context.Routes
                .Include(r => r.RouteStops)
                .Include(r => r.Trips)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
                return NotFound();

            // ❗ Không cho xóa nếu đã có chuyến
            if (route.Trips.Any())
            {
                return BadRequest("Không thể xóa tuyến vì đã có chuyến xe liên quan");
            }


            _context.RouteStops.RemoveRange(route.RouteStops);


            _context.Routes.Remove(route);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xóa tuyến thành công"
            });
        }
    }
}
