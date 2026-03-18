using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GameBuddy.API.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task<bool> SendContactNotificationAsync(string fromName, string fromEmail, string message)
    {
        var apiKey  = config["Resend:ApiKey"];
        var toEmail = config["Resend:ToEmail"];

        if (string.IsNullOrEmpty(apiKey) || apiKey == "re_TU_API_KEY")
        {
            logger.LogWarning("Resend no configurado. Mensaje guardado en BD pero no se envió email.");
            return false;
        }

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from    = "GameBuddy <onboarding@resend.dev>",
                to      = new[] { toEmail },
                subject = $"[GameBuddy] Nuevo mensaje de {fromName}",
                text    = $"""
                           Nuevo mensaje de contacto en GameBuddy:

                           Nombre:  {fromName}
                           Email:   {fromEmail}
                           Mensaje:
                           {message}

                           ---
                           Responder a: {fromEmail}
                           """
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await http.PostAsync("https://api.resend.com/emails", content);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Email enviado via Resend desde {Email}", fromEmail);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Resend error {Status}: {Error}", response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar email via Resend");
            return false;
        }
    }
}
