using System.ComponentModel.DataAnnotations;

namespace CampusRoomBackend.DTOs
{
    public class BookingRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public string Purpose { get; set; } = string.Empty;
    }
}