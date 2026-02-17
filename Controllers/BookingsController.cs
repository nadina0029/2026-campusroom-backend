using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusRoomBackend.Data;
using CampusRoomBackend.Models;
using CampusRoomBackend.DTOs; // Pastikan namespace DTO sesuai project kamu

namespace CampusRoomBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. POST: Ajukan Peminjaman (Mahasiswa/Admin)
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Booking>> CreateBooking(BookingRequest request)
        {
            // A. Validasi Waktu
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest("Waktu selesai harus lebih besar dari waktu mulai.");
            }
            if (request.StartTime < DateTime.Now)
            {
                return BadRequest("Tidak bisa meminjam untuk waktu masa lalu.");
            }

            // B. Cek Ketersediaan Ruangan
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room == null) return NotFound("Ruangan tidak ditemukan.");

            // C. LOGIKA ANTI-TABRAKAN (Conflict Check)
            // Rumus: (StartA < EndB) && (EndA > StartB)
            // Kita abaikan booking yang statusnya "Rejected"
            var conflict = await _context.Bookings
                .AnyAsync(b => b.RoomId == request.RoomId &&
                               b.Status != "Rejected" &&
                               b.StartTime < request.EndTime &&
                               b.EndTime > request.StartTime);

            if (conflict)
            {
                // Return 400 Bad Request jika tabrakan
                return BadRequest("Ruangan sudah dipesan di jam tersebut. Silakan pilih waktu lain.");
            }

            // D. Ambil ID User dari Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("User ID tidak valid.");

            int userId = int.Parse(userIdString);

            // E. Simpan ke Database
            var newBooking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Purpose = request.Purpose,
                Status = "Pending" // Default status
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyBookings), new { id = newBooking.Id }, newBooking);
        }

        // ==========================================
        // 2. GET: Lihat Booking Saya (Mahasiswa)
        // ==========================================
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Booking>>> GetMyBookings(
            [FromQuery] string? search,
            [FromQuery] string? status
        )
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            int userId = int.Parse(userIdString);

            // QUERY PENTING: Include Room agar nama ruangan muncul
            var query = _context.Bookings
                .Include(b => b.Room) // <--- JOIN KE TABEL ROOM
                .Where(b => b.UserId == userId)
                .AsQueryable();

            // Filter Search (Nama Ruangan / Keperluan)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    (b.Room != null && b.Room.Name.Contains(search)) || // Tambahkan pengecekan null di sini
                    b.Purpose.Contains(search));
            }

            // Filter Status
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(b => b.Status == status);
            }

            return await query
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        // ==========================================
        // 3. GET: Lihat Semua Booking (ADMIN)
        // ==========================================
        [HttpGet("all")]
        // [Authorize(Roles = "Admin")] // Aktifkan jika sudah setup Roles di JWT
        public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings(
            [FromQuery] string? search,
            [FromQuery] string? status
        )
        {
            // QUERY PENTING: Include User & Room untuk info lengkap
            var query = _context.Bookings
                .Include(b => b.User) // <--- JOIN KE TABEL USER (Lihat nama peminjam)
                .Include(b => b.Room) // <--- JOIN KE TABEL ROOM (Lihat nama ruangan)
                .AsQueryable();

            // Filter Search (Nama User / Nama Ruangan / Keperluan)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    (b.User != null && b.User.Username.Contains(search)) || // Cek dulu: User gak boleh null
                    (b.Room != null && b.Room.Name.Contains(search)) ||     // Cek dulu: Room gak boleh null
                    b.Purpose.Contains(search));
            }

            // Filter Status
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(b => b.Status == status);
            }

            var bookings = await query
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            return bookings;
        }

        // ==========================================
        // 4. PUT: Update Status (Admin Approve/Reject)
        // ==========================================
        [HttpPut("{id}/status")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] BookingStatusRequest request)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound("Booking tidak ditemukan.");

            // Validasi Input
            if (request.Status != "Approved" && request.Status != "Rejected" && request.Status != "Pending")
            {
                return BadRequest("Status tidak valid.");
            }

            booking.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status berhasil diubah menjadi {request.Status}", data = booking });
        }
    }
}