using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserRolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserRolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetAll()
        {
            var userRoles = await _context.UserRoles
                .AsNoTracking()
                .ToListAsync();

            return Ok(userRoles);
        }

        [HttpGet("{userId:int}/{roleId:int}")]
        public async Task<ActionResult<UserRole>> GetById(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                return NotFound();
            }

            return Ok(userRole);
        }

        [HttpPost]
        public async Task<ActionResult<UserRole>> Create(UserRole userRole)
        {
            var exists = await _context.UserRoles.AnyAsync(
                ur => ur.UserId == userRole.UserId && ur.RoleId == userRole.RoleId);

            if (exists)
            {
                return Conflict("UserRole already exists.");
            }

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { userId = userRole.UserId, roleId = userRole.RoleId },
                userRole);
        }

        [HttpPut("{userId:int}/{roleId:int}")]
        public async Task<IActionResult> Update(int userId, int roleId, UserRole userRole)
        {
            if (userId != userRole.UserId || roleId != userRole.RoleId)
            {
                return BadRequest("Key mismatch.");
            }

            _context.Entry(userRole).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.UserRoles.AnyAsync(
                    ur => ur.UserId == userId && ur.RoleId == roleId);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{userId:int}/{roleId:int}")]
        public async Task<IActionResult> Delete(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                return NotFound();
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
