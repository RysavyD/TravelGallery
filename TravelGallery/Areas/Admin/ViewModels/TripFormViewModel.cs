using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Areas.Admin.ViewModels;

public class TripFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Zadejte název výletu")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zadejte datum výletu")]
    public DateTime Date { get; set; } = DateTime.Today;

    public string Description { get; set; } = string.Empty;
}
