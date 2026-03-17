using GameBuddy.API.Data;
using GameBuddy.API.DTOs;
using GameBuddy.API.Models;
using GameBuddy.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameBuddy.API.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController(AppDbContext db, EmailService emailService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage(ContactMessageDto dto)
    {
        // 1. Guardar en la BD
        var contact = new ContactMessage
        {
            Name    = dto.Name.Trim(),
            Email   = dto.Email.Trim().ToLower(),
            Message = dto.Message.Trim(),
        };

        db.ContactMessages.Add(contact);
        await db.SaveChangesAsync();

        // 2. Intentar enviar email (no bloquea si falla)
        contact.EmailSent = await emailService.SendContactNotificationAsync(
            contact.Name, contact.Email, contact.Message);

        await db.SaveChangesAsync();

        return Ok(new { message = "Mensaje recibido. ¡Pronto te responderemos!" });
    }
}
