using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Areas.Admin.ViewModels;

public class ChangePasswordViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zadejte nové heslo.")]
    [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nové heslo")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrďte nové heslo.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Hesla se neshodují.")]
    [Display(Name = "Potvrzení hesla")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
