namespace GameBuddy.API.Models;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Nickname { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string GamesJson { get; set; } = "[]";
    public string Schedule { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
