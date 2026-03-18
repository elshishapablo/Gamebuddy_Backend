using System.Net;
using System.Net.Mail;

namespace GameBuddy.API.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task<bool> SendContactNotificationAsync(string fromName, string fromEmail, string message)
    {
        var smtpHost = config["Smtp:Host"];
        var smtpPort = int.Parse(config["Smtp:Port"] ?? "587");
        var smtpUser = config["Smtp:Username"];
        var smtpPass = config["Smtp:Password"];
        var toEmail  = config["Smtp:ToEmail"];

        if (string.IsNullOrEmpty(smtpUser) || smtpUser == "TU_CORREO@gmail.com")
        {
            logger.LogWarning("SMTP no configurado. El mensaje se guardó en la BD pero no se envió email.");
            return false;
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                Timeout = 10000, // 10 segundos máximo
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, "GameBuddy"),
                Subject = $"[GameBuddy] Nuevo mensaje de {fromName}",
                Body = $"""
                        Tienes un nuevo mensaje de contacto en GameBuddy:

                        Nombre:  {fromName}
                        Email:   {fromEmail}
                        Mensaje:
                        {message}

                        ---
                        Respondele directamente a: {fromEmail}
                        """,
                IsBodyHtml = false,
            };

            mail.To.Add(toEmail!);

            await client.SendMailAsync(mail);
            logger.LogInformation("Email de contacto enviado desde {Email}", fromEmail);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar email de contacto");
            return false;
        }
    }
}
