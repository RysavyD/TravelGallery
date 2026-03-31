namespace TravelGallery.Models;

public class TravelGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
}
