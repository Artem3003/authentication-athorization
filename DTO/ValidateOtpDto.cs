
using System.ComponentModel.DataAnnotations;

namespace authentication_athorization.DTO;

public class ValidateOtpDto
{
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Code { get; set; }
}
