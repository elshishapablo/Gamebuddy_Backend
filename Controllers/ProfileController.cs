using System.Security.Claims;
using System.Text.Json;
using GameBuddy.API.Data;
using GameBuddy.API.DTOs;
using GameBuddy.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBuddy.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(AppDbContext db) : ControllerBase
{
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET api/profile → devuelve el perfil del usuario autenticado
    [HttpGet]
    public async Task<ActionResult<ProfileResponseDto>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            return NotFound(new { message = "Perfil no encontrado." });

        return Ok(new ProfileResponseDto
        {
            Nickname = profile.Nickname,
            Platform = profile.Platform,
            Games = JsonSerializer.Deserialize<List<string>>(profile.GamesJson) ?? new(),
            Schedule = profile.Schedule
        });
    }

    // POST api/profile → crea o actualiza el perfil del usuario autenticado
    [HttpPost]
    public async Task<ActionResult<ProfileResponseDto>> SaveProfile(SaveProfileDto dto)
    {
        var userId = GetUserId();
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(profile);
        }

        profile.Nickname = dto.Nickname;
        profile.Platform = dto.Platform;
        profile.GamesJson = JsonSerializer.Serialize(dto.Games);
        profile.Schedule = dto.Schedule;
        profile.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(new ProfileResponseDto
        {
            Nickname = profile.Nickname,
            Platform = profile.Platform,
            Games = dto.Games,
            Schedule = profile.Schedule
        });
    }
}
