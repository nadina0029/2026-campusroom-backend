using System.ComponentModel.DataAnnotations;

namespace CampusRoomBackend.DTOs
{
    public class BookingStatusRequest
    {
        // Isinya cuma boleh "Approved" atau "Rejected"
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}