
using System.ComponentModel.DataAnnotations;

namespace authentication_athorization.DTO;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? ClientURI { get; set; }
}
