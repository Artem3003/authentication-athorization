using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;

namespace IdentityUserRegistration.DTO;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }

    public string? Email { get; set; }
    public string? Token { get; set; }
}