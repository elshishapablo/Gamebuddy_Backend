using System.ComponentModel.DataAnnotations;

namespace GameBuddy.API.DTOs;

public class SaveProfileDto
{
    [Required]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    public string Platform { get; set; } = string.Empty;

    [Required]
    public List<string> Games { get; set; } = new();

    [Required]
    public string Schedule { get; set; } = string.Empty;
}

public class ProfileResponseDto
{
    public string Nickname { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public List<string> Games { get; set; } = new();
    public string Schedule { get; set; } = string.Empty;
}
