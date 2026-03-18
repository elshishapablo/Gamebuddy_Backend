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

            var html = $"""
                <!DOCTYPE html>
                <html>
                <body style="margin:0;padding:0;background:#0f0f13;font-family:'Segoe UI',Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background:#0f0f13;padding:40px 20px;">
                    <tr>
                      <td align="center">
                        <table width="560" cellpadding="0" cellspacing="0" style="background:#1a1a2e;border-radius:16px;overflow:hidden;border:1px solid #2a2a3e;">

                          <!-- Header -->
                          <tr>
                            <td style="background:linear-gradient(135deg,#6c63ff,#a78bfa);padding:32px;text-align:center;">
                              <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:-0.5px;">🎮 GameBuddy</h1>
                              <p style="margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px;">Nuevo mensaje de contacto</p>
                            </td>
                          </tr>

                          <!-- Body -->
                          <tr>
                            <td style="padding:32px;">

                              <!-- Info cards -->
                              <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;">
                                <tr>
                                  <td style="padding:12px 16px;background:#0f0f13;border-radius:10px;border-left:3px solid #6c63ff;">
                                    <p style="margin:0;color:#8888aa;font-size:11px;text-transform:uppercase;letter-spacing:1px;">Nombre</p>
                                    <p style="margin:4px 0 0;color:#e8e8f0;font-size:16px;font-weight:600;">{fromName}</p>
                                  </td>
                                </tr>
                                <tr><td style="height:10px;"></td></tr>
                                <tr>
                                  <td style="padding:12px 16px;background:#0f0f13;border-radius:10px;border-left:3px solid #a78bfa;">
                                    <p style="margin:0;color:#8888aa;font-size:11px;text-transform:uppercase;letter-spacing:1px;">Email</p>
                                    <p style="margin:4px 0 0;font-size:16px;">
                                      <a href="mailto:{fromEmail}" style="color:#a78bfa;text-decoration:none;font-weight:600;">{fromEmail}</a>
                                    </p>
                                  </td>
                                </tr>
                              </table>

                              <!-- Message -->
                              <p style="margin:0 0 10px;color:#8888aa;font-size:11px;text-transform:uppercase;letter-spacing:1px;">Mensaje</p>
                              <div style="background:#0f0f13;border-radius:10px;padding:20px;border:1px solid #2a2a3e;">
                                <p style="margin:0;color:#c8c8d8;font-size:15px;line-height:1.7;white-space:pre-wrap;">{message}</p>
                              </div>

                              <!-- CTA -->
                              <table width="100%" cellpadding="0" cellspacing="0" style="margin-top:28px;">
                                <tr>
                                  <td align="center">
                                    <a href="mailto:{fromEmail}" style="display:inline-block;background:linear-gradient(135deg,#6c63ff,#a78bfa);color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:10px;font-size:15px;font-weight:600;">
                                      Responder a {fromName}
                                    </a>
                                  </td>
                                </tr>
                              </table>

                            </td>
                          </tr>

                          <!-- Footer -->
                          <tr>
                            <td style="padding:20px 32px;border-top:1px solid #2a2a3e;text-align:center;">
                              <p style="margin:0;color:#555566;font-size:12px;">GameBuddy · Encuentra tu compañero de juego</p>
                            </td>
                          </tr>

                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;

            var payload = new
            {
                from    = "GameBuddy <onboarding@resend.dev>",
                to      = new[] { toEmail },
                subject = $"[GameBuddy] Nuevo mensaje de {fromName}",
                html,
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
