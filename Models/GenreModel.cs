using EventManager.Models;
using System.ComponentModel.DataAnnotations;

public class GenreModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    // Navigation property
    public ICollection<EventModel> Events { get; set; } = null!;

}
