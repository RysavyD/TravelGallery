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

    public string? Description { get; set; }

    /// <summary>Čárkou oddělené názvy tagů, např. "Hory, Zima, Slovensko"</summary>
    public string? TagNames { get; set; }

    [Range(-90, 90, ErrorMessage = "Zeměpisná šířka musí být mezi -90 a 90")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Zeměpisná délka musí být mezi -180 a 180")]
    public double? Longitude { get; set; }
}
