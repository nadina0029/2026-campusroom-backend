using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusRoomBackend.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        // 1. Siapa yang meminjam? (Relasi ke User)
        [Required]
        public int UserId { get; set; }
        // Ini trik agar kita bisa ambil data User lengkap (nama, email, dll)
        [ForeignKey("UserId")] 
        public User? User { get; set; }

        // 2. Ruangan apa yang dipinjam? (Relasi ke Room)
        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // 3. Kapan?
        [Required]
        public DateTime StartTime { get; set; } // Tgl & Jam Mulai
        [Required]
        public DateTime EndTime { get; set; }   // Tgl & Jam Selesai

        // 4. Keperluan apa?
        public string Purpose { get; set; } = string.Empty;

        // 5. Statusnya gimana? (Pending, Approved, Rejected)
        // Kita set default "Pending"
        public string Status { get; set; } = "Pending";
    }
}