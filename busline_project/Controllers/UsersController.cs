using busline_project.Data;
using busline_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private static readonly HashSet<int> AllowedRoleIds = new() { 1, 2, 3 };
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(UserUpsertDto dto)
        {
            var roleIds = NormalizeRoleIds(dto.RoleIds);
            var invalidRoleIds = roleIds.Where(id => !AllowedRoleIds.Contains(id)).ToList();
            if (invalidRoleIds.Count > 0)
            {
                return BadRequest($"RoleIds not allowed: {string.Join(", ", invalidRoleIds)}. Allowed: 1=Admin, 2=Customer, 3=Staff.");
            }

            if (roleIds.Count > 0)
            {
                var existingRoleIds = await _context.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .Select(r => r.Id)
                    .ToListAsync();

                var missingRoleIds = roleIds.Except(existingRoleIds).ToList();
                if (missingRoleIds.Count > 0)
                {
                    return BadRequest($"RoleIds not found: {string.Join(", ", missingRoleIds)}.");
                }
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = dto.PasswordHash,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Status = dto.Status
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (roleIds.Count > 0)
            {
                var userRoles = roleIds
                    .Select(roleId => new UserRole { UserId = user.Id, RoleId = roleId })
                    .ToList();

                _context.UserRoles.AddRange(userRoles);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UserUpsertDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.Username = dto.Username;
            user.PasswordHash = dto.PasswordHash;
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;

            if (dto.RoleIds != null)
            {
                var roleIds = NormalizeRoleIds(dto.RoleIds);
                var invalidRoleIds = roleIds.Where(rid => !AllowedRoleIds.Contains(rid)).ToList();
                if (invalidRoleIds.Count > 0)
                {
                    return BadRequest($"RoleIds not allowed: {string.Join(", ", invalidRoleIds)}. Allowed: 1=Admin, 2=Customer, 3=Staff.");
                }

                if (roleIds.Count > 0)
                {
                    var existingRoleIds = await _context.Roles
                        .Where(r => roleIds.Contains(r.Id))
                        .Select(r => r.Id)
                        .ToListAsync();

                    var missingRoleIds = roleIds.Except(existingRoleIds).ToList();
                    if (missingRoleIds.Count > 0)
                    {
                        return BadRequest($"RoleIds not found: {string.Join(", ", missingRoleIds)}.");
                    }
                }

                _context.UserRoles.RemoveRange(user.UserRoles);
                var newUserRoles = roleIds
                    .Select(roleId => new UserRole { UserId = user.Id, RoleId = roleId })
                    .ToList();

                _context.UserRoles.AddRange(newUserRoles);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static List<int> NormalizeRoleIds(IEnumerable<int>? roleIds)
        {
            return roleIds == null ? new List<int>() : roleIds.Distinct().ToList();
        }

        public class UserUpsertDto
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public UserStatus Status { get; set; } = UserStatus.Active;
            public List<int>? RoleIds { get; set; }
        }
    }
}
