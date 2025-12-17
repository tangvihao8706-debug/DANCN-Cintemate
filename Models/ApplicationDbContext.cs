using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventManager.Models;
using Microsoft.AspNetCore.Identity;        
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventModel> Events { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<EventRating> EventRatings { get; set; }
    public DbSet<Registration> Registrations { get; set; } = default!;
    public DbSet<GenreModel> Genres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 👇 Thêm ràng buộc duy nhất cho User + Event
        modelBuilder.Entity<Registration>()
            .HasIndex(r => new { r.EventId, r.UserId })
            .IsUnique();

        modelBuilder.Entity<GenreModel>().HasData(
            new GenreModel { Id = 1, Name = "Kinh dị" },
            new GenreModel { Id = 2, Name = "Hài" },
            new GenreModel { Id = 3, Name = "Hoạt Hình" },
            new GenreModel { Id = 4, Name = "Trinh Thám" },
            new GenreModel { Id = 5, Name = "Viễn tưởng" }
        );

        modelBuilder.Entity<EventModel>().HasData(
            new EventModel
            {

                Id = 1,
                Title = "BlackPink World Tour",
                Description = "Buổi biểu diễn đỉnh cao của BlackPink tại Hà Nội!",
                Location = "Hà Nội",
                Date = new DateTime(2025, 6, 30),
                TicketPrice = 500_000_000,
                GenreId = 1,

                // Các trường không bắt buộc (có thể thêm nếu muốn)
                MediaPath = null,
                StartTime = "18:00",
                EndTime = "22:30",
                Artists = "BlackPink",
                MapEmbedUrl = null,
                FacebookUrl = null,
                YoutubeUrl = null,
                ScheduleHtml = null
            }
        );
    }


}
