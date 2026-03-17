using System.ComponentModel.DataAnnotations;

namespace GameBuddy.API.DTOs;

public class ContactMessageDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string Message { get; set; } = string.Empty;
}
