using Microsoft.AspNetCore.Mvc;
using busline_project.Data;
using busline_project.Models;
using busline_project.Dtos;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace busline_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Username and password are required" });

            // check unique username/email
            if (await _db.Users.AnyAsync(u => u.Username == req.Username))
                return Conflict(new { message = "Username already taken" });
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return Conflict(new { message = "Email already registered" });

            var hashed = BCrypt.Net.BCrypt.EnhancedHashPassword(req.Password);

            var user = new User
            {
                Username = req.Username,
                PasswordHash = hashed,
                FullName = req.FullName,
                Email = req.Email,
                Phone = req.Phone,
                Status = UserStatus.Active
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return CreatedAtAction(null, new AuthResponse(user.Id, user.Username, user.FullName, user.Email, token));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Identifier) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Identifier and password are required" });

            var identifier = req.Identifier.Trim();

            // Allow login by username, email or phone
            var user = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier || u.Phone == identifier);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var verified = BCrypt.Net.BCrypt.EnhancedVerify(req.Password, user.PasswordHash);
            if (!verified)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse(user.Id, user.Username, user.FullName, user.Email, token));
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT Key not configured");
            var issuer = jwtSection.GetValue<string>("Issuer") ?? "busline";
            var audience = jwtSection.GetValue<string>("Audience") ?? "busline_clients";
            var expiresInMinutes = jwtSection.GetValue<int?>("ExpiresMinutes") ?? 60;

            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim("fullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            // add roles
            foreach (var ur in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.RoleName));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
