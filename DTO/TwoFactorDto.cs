using System.ComponentModel.DataAnnotations;

namespace authentication_athorization.DTO;

public class TwoFactorDto
{
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Provider { get; set; }
    [Required]
    public string? Token { get; set; }
}