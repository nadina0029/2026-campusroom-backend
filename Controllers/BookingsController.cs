using System.Security.Claims; // Wajib untuk baca Token
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusRoomBackend.Data;
using CampusRoomBackend.Models;
using CampusRoomBackend.DTOs;

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

        // 1. POST: Ajukan Peminjaman (Hanya User Login)
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

            // B. Cek Apakah Ruangan Ada?
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room == null) return NotFound("Ruangan tidak ditemukan.");

            // C. LOGIKA ANTI-TABRAKAN (The Core Logic)
            // Kita cari booking lain di ruangan yang sama, yang statusnya BUKAN Rejected
            // Rumus tabrakan: (StartA < EndB) && (EndA > StartB)
            var conflict = await _context.Bookings
                .AnyAsync(b => b.RoomId == request.RoomId &&
                               b.Status != "Rejected" &&
                               b.StartTime < request.EndTime &&
                               b.EndTime > request.StartTime);

            if (conflict)
            {
                return BadRequest("Ruangan sudah dipesan di jam tersebut. Silakan pilih waktu lain.");
            }

            // D. Ambil ID User dari Token (Otomatis)
            // Pastikan Token mengandung claim "id" atau NameIdentifier
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("User ID tidak ditemukan di token.");
            
            int userId = int.Parse(userIdString);

            // E. Simpan ke Database
            var newBooking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Purpose = request.Purpose,
                Status = "Pending" // Default status menunggu Admin
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyBookings), new { id = newBooking.Id }, newBooking);
        }

        // 2. GET: Lihat Booking Saya (Mahasiswa cuma lihat punya sendiri)
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Booking>>> GetMyBookings()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            int userId = int.Parse(userIdString);

            // Tampilkan booking user tersebut + detail ruangannya
            return await _context.Bookings
                .Include(b => b.Room) // Join tabel Room
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        // 3. GET: Lihat Semua Booking (Khusus Admin)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings()
        {
            return await _context.Bookings
                .Include(b => b.User) // Lihat siapa yang pinjam
                .Include(b => b.Room) // Lihat ruangan apa
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }
    }
}