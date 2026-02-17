using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusRoomBackend.Data;
using CampusRoomBackend.Models;
using Microsoft.AspNetCore.Authorization; // Pastikan ini ada

namespace CampusRoomBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: Semua orang boleh lihat + FITUR SEARCH
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms([FromQuery] string? search)
        {
            // A. Siapkan Query
            var query = _context.Rooms.AsQueryable();

            // B. Logika Search
            if (!string.IsNullOrEmpty(search))
            {
                // Cari berdasarkan Nama Ruangan
                // (Kamu bisa tambah kondisi lain, misal deskripsi kalau ada)
                query = query.Where(r => r.Name.Contains(search));
            }

            // C. Eksekusi
            return await query.ToListAsync();
        }

        // 2. GET Detail: Semua orang yang sudah login boleh lihat
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound("Ruangan tidak ditemukan");
            return room;
        }

        // 3. POST: HANYA ADMIN yang boleh tambah
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Room>> PostRoom(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetRoom", new { id = room.Id }, room);
        }

        // 4. PUT: HANYA ADMIN yang boleh edit
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Room room)
        {
            if (id != room.Id) return BadRequest();

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // handle error
            }
            return NoContent();
        }

        // 5. DELETE: HANYA ADMIN yang boleh hapus
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}