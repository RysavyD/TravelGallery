using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Models.Api;

public class LoginRequest
{
    [Required] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
