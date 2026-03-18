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

        // 2. Enviar email en segundo plano — responde al cliente sin esperar
        var name    = contact.Name;
        var email   = contact.Email;
        var message = contact.Message;
        _ = Task.Run(() => emailService.SendContactNotificationAsync(name, email, message));

        return Ok(new { message = "Mensaje recibido. ¡Pronto te responderemos!" });
    }
}
