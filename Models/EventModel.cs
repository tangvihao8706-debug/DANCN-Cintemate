using Microsoft.AspNetCore.Identity;
using EventManager.Data;
using System.ComponentModel.DataAnnotations;
using EventManager.Models;
namespace EventManager.Models
{
    public class EventModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = default!;
        [Required]
        public string Description { get; set; } = default!;
        [Required]
        public string Location { get; set; } = default!;
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TicketPrice { get; set; }

        // Khóa ngoại
        [Required(ErrorMessage = "Vui lòng chọn thể loại sự kiện")]
        [Range(1, int.MaxValue, ErrorMessage = "Thể loại không hợp lệ")]
        public int GenreId { get; set; }

        // Navigation property
        public GenreModel? Genre { get; set; }
        // 🔥 Thêm thuộc tính lưu tên file ảnh/video
        public string? MediaPath { get; set; }
        
        public string? StartTime { get; set; }       // ví dụ: 18:00
        public string? EndTime { get; set; }         // ví dụ: 23:00
        public string? Artists { get; set; }         // DJ biểu diễn
        public string? MapEmbedUrl { get; set; }     // iframe Google Maps
        public string? FacebookUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? ScheduleHtml { get; set; }    // HTML lịch trình
        public bool IsFeatured { get; set; }
        // ✅ Số người có thể tham gia tối đa
        [Required(ErrorMessage = "Vui lòng nhập số lượng người tham gia tối đa")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người tham gia tối đa phải lớn hơn 0")]
        public int MaxParticipants { get; set; }

        // ✅ Số người đã đăng ký
        public int CurrentParticipants { get; set; } = 0;

    }

}
