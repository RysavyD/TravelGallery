using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Areas.Admin.ViewModels;

public class UserCreateViewModel
{
    [Required(ErrorMessage = "Zadejte e-mail")]
    [EmailAddress(ErrorMessage = "Neplatný formát e-mailu")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zadejte jméno")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zadejte heslo")]
    [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}
