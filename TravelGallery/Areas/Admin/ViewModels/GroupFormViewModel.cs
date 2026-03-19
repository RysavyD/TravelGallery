using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelGallery.Areas.Admin.ViewModels;

public class GroupFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Zadejte název skupiny")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public List<int> SelectedTripIds { get; set; } = new();
    public List<string> SelectedUserIds { get; set; } = new();

    public List<SelectListItem> AllTrips { get; set; } = new();
    public List<SelectListItem> AllUsers { get; set; } = new();
}
