using System.ComponentModel.DataAnnotations;

namespace TravelGallery.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Zadejte e-mail")]
    [EmailAddress(ErrorMessage = "Neplatný formát e-mailu")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zadejte heslo")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
