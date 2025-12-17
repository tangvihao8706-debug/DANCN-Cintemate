using System.ComponentModel.DataAnnotations.Schema;
using EventManager.Models;
using Microsoft.AspNetCore.Identity;
namespace EventManager.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        // Khóa ngoại
        [ForeignKey("EventId")]
        public EventModel Event { get; set; } = default!; // 👈 Quan hệ đến EventModel

        public string UserId { get; set; } = default!;
        public bool IsPaid { get; set; }
        public string? SelectedArea { get; set; }
        // ✅ Thêm chỗ ngồi
        public string? SeatNumber { get; set; }
        // ✅ Thêm trường này
        public string? QrCodeBase64 { get; set; }
        public DateTime RegisteredAt { get; set; }

        // Quan hệ tới người dùng (nếu có dùng Identity)
        // public ApplicationUser? User { get; set; }
    }
}
