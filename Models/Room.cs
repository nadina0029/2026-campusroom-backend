using System.ComponentModel.DataAnnotations;

namespace CampusRoomBackend.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Contoh: "Lab Komputer 1"

        public int Capacity { get; set; } // Contoh: 30 orang

        public string Facilities { get; set; } = string.Empty; // Contoh: "AC, Proyektor, WiFi"

        public bool IsAvailable { get; set; } = true; // Status ruangan bisa dipinjam atau sedang perbaikan
    }
}