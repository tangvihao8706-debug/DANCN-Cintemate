using System;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [Display(Name = "Poster URL")]
        public string PosterUrl { get; set; }

        public string Description { get; set; }

        [Range(0, 10)]
        public double Rating { get; set; }

        [Display(Name = "Release Date")]
        public DateTime? ReleaseDate { get; set; }

        // Mới: để quản lý phim hot, hiển thị, v.v.
        [Display(Name = "Thể loại")]
        public string Genre { get; set; }

        [Display(Name = "Thời lượng (phút)")]
        public int? Duration { get; set; }

        [Display(Name = "Phim Hot")]
        public bool IsHot { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
