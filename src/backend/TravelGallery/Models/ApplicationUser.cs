using Microsoft.AspNetCore.Identity;

namespace TravelGallery.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<TravelGroup> Groups { get; set; } = new List<TravelGroup>();
}
