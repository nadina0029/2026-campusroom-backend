using Microsoft.AspNetCore.Mvc;
using CampusRoomBackend.Data;
using CampusRoomBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CampusRoomBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Class kecil untuk menampung data yang dikirim user
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Cari user di database berdasarkan Username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // 2. Cek apakah user ada & password cocok
            if (user == null || user.Password != request.Password)
            {
                return Unauthorized("Username atau Password salah, Bos!");
            }

            // 3. Kalau cocok, kita buatkan Karcis (Token JWT)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Simpan ID
                    new Claim(ClaimTypes.Name, user.Username), // Simpan Username
                    new Claim(ClaimTypes.Role, user.Role) // Simpan Role (PENTING!)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token berlaku 1 jam
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 4. Kirim Token balikan ke User
            return Ok(new { 
                Token = tokenString, 
                Message = "Login Berhasil!",
                Role = user.Role 
            });
        }
    }
}