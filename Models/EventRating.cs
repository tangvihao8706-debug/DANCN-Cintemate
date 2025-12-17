using System;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Models
{
    public class EventRating
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public string UserName { get; set; } // hoặc UserId nếu dùng Identity

        public DateTime CreatedAt { get; set; }

        public EventModel Event { get; set; }
    }
}
