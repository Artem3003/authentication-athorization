
using System.ComponentModel.DataAnnotations;

namespace IdentityUserRegistration.DTO;

public class ValidateOtpDto
{
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Code { get; set; }
}
