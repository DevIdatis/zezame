using Resend;
using System.Xml.Linq;

namespace Zezame.Api.Email {
    /// <summary>
    /// Implementación de desarrollo: no envía nada, escribe el email (destinatario,
    /// asunto y cuerpo con el enlace/token) en el log. Permite probar los flujos de
    /// verificación y reset en local sin un proveedor de correo.
    /// </summary>
    public class LoggingEmailSender(ILogger<LoggingEmailSender> logger, IResend resend) : IEmailSender {
        private readonly ILogger<LoggingEmailSender> _logger = logger;
        private readonly IResend _resend = resend;

        public async Task SendAsync(string toEmail, string subject, string invitationUrl, string organization = "") {

            var message = new EmailMessage();
            // El "from" debe usar tu dominio verificado
            message.From = "Zezame <no-reply@notifications.merienel.com>";
            message.To.Add(toEmail);
            message.Subject = subject;//$"Invitación al proyecto: {projectName}";
            message.HtmlBody = GetInvitationTemplate(organization, invitationUrl);
            await _resend.EmailSendAsync(message);

            _logger.LogInformation(
                "[EMAIL-DEV] Para: {To}\nAsunto: {Subject}\n{Body}",
                toEmail, subject, invitationUrl);
            //return Task.CompletedTask;
        }

        public string GetInvitationTemplate(string organization, string invitationUrl) {
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin: 0; padding: 0; background-color: #f7f7f7; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f7f7f7; padding: 40px 20px;'>
            <tr>
                <td align='center'>
                    <!-- Tarjeta principal -->
                    <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05);'>
                        <!-- Barra superior con degradado -->
                        <tr>
                            <td style='height: 6px; background-image: linear-gradient(to right, #FF7A59, #FFD166);'></td>
                        </tr>
                        
                        <!-- Cabecera -->
                        <tr>
                            <td align='center' style='padding: 40px 20px 20px 20px;'>
                                <div style='background-color: #FFF5F2; width: 64px; height: 64px; border-radius: 16px; line-height: 64px; font-size: 32px; margin: 0 auto 20px auto;'>
                                    🎯
                                </div>
                                <h1 style='margin: 0; color: #1a1a1a; font-size: 24px; font-weight: 700;'>¡Nueva invitación a organización!</h1>
                            </td>
                        </tr>

                        <!-- Contenido -->
                        <tr>
                            <td style='padding: 0 40px 30px 40px; color: #4a4a4a; font-size: 16px; line-height: 1.6;'>
                                <p style='margin: 0 0 20px 0;'><strong>Hola!!</strong></p>
                                <p style='margin: 0 0 20px 0;'>Te han añadido a la organización <strong>{organization}</strong>. Ya puedes empezar a colaborar, ver tareas y registrar tu tiempo.</p>
                                
                                <!-- Botón -->
                                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 30px 0;'>
                                    <tr>
                                        <td align='center'>
                                            <a href='{invitationUrl}' style='display: inline-block; background-image: linear-gradient(to right, #FF7A59, #FFB85C); color: #ffffff; text-decoration: none; font-weight: 600; font-size: 16px; padding: 14px 32px; border-radius: 12px;'>
                                                Entrar al proyecto
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                                
                                <p style='margin: 0; font-size: 14px; color: #888888;'>Si no esperabas esta invitación, puedes ignorar este correo de forma segura.</p>
                            </td>
                        </tr>
                    </table>
                    
                    <!-- Footer -->
                    <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; margin-top: 20px;'>
                        <tr>
                            <td align='center' style='color: #999999; font-size: 12px; padding: 20px;'>
                                Enviado con ❤️ por <strong>Zezame</strong><br>
                                © {DateTime.Now.Year} Zezame. Todos los derechos reservados.
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }

        public string GetVerificationTemplate(string userName, string verificationUrl) {
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin: 0; padding: 0; background-color: #f7f7f7; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f7f7f7; padding: 40px 20px;'>
            <tr>
                <td align='center'>
                    <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05);'>
                        <!-- Barra superior con degradado -->
                        <tr>
                            <td style='height: 6px; background-image: linear-gradient(to right, #22D3EE, #FFD166);'></td>
                        </tr>
                        
                        <tr>
                            <td align='center' style='padding: 40px 20px 20px 20px;'>
                                <div style='background-color: #ECFEFF; width: 64px; height: 64px; border-radius: 16px; line-height: 64px; font-size: 32px; margin: 0 auto 20px auto;'>
                                    ✉️
                                </div>
                                <h1 style='margin: 0; color: #1a1a1a; font-size: 24px; font-weight: 700;'>Verifica tu correo</h1>
                            </td>
                        </tr>

                        <tr>
                            <td style='padding: 0 40px 30px 40px; color: #4a4a4a; font-size: 16px; line-height: 1.6;'>
                                <p style='margin: 0 0 20px 0;'>¡Bienvenido a Zezame, <strong>{userName}</strong>!</p>
                                <p style='margin: 0 0 20px 0;'>Estás a un solo paso de empezar a organizar tus proyectos. Por favor, confirma tu dirección de correo electrónico haciendo clic en el botón de abajo:</p>
                                
                                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 30px 0;'>
                                    <tr>
                                        <td align='center'>
                                            <a href='{verificationUrl}' style='display: inline-block; background-color: #1a1a1a; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 16px; padding: 14px 32px; border-radius: 12px;'>
                                                Verificar mi cuenta
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                                
                                <p style='margin: 0 0 10px 0; font-size: 14px; color: #888888;'>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                                <p style='margin: 0; font-size: 12px; color: #22D3EE; word-break: break-all;'>{verificationUrl}</p>
                            </td>
                        </tr>
                    </table>
                    
                    <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; margin-top: 20px;'>
                        <tr>
                            <td align='center' style='color: #999999; font-size: 12px; padding: 20px;'>
                                Enviado con ❤️ por <strong>Zezame</strong><br>
                                © {DateTime.Now.Year} Zezame. Todos los derechos reservados.
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }

        public string GetPasswordResetTemplate(string userName, string resetUrl) {
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin: 0; padding: 0; background-color: #f7f7f7; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
        <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f7f7f7; padding: 40px 20px;'>
            <tr>
                <td align='center'>
                    <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05);'>
                        <!-- Barra superior con degradado -->
                        <tr>
                            <td style='height: 6px; background-image: linear-gradient(to right, #FF7A59, #EF4444);'></td>
                        </tr>
                        
                        <tr>
                            <td align='center' style='padding: 40px 20px 20px 20px;'>
                                <div style='background-color: #FEF2F2; width: 64px; height: 64px; border-radius: 16px; line-height: 64px; font-size: 32px; margin: 0 auto 20px auto;'>
                                    🔐
                                </div>
                                <h1 style='margin: 0; color: #1a1a1a; font-size: 24px; font-weight: 700;'>Restablecer contraseña</h1>
                            </td>
                        </tr>

                        <tr>
                            <td style='padding: 0 40px 30px 40px; color: #4a4a4a; font-size: 16px; line-height: 1.6;'>
                                <p style='margin: 0 0 20px 0;'>Hola <strong>{userName}</strong>,</p>
                                <p style='margin: 0 0 20px 0;'>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en Zezame. Si fuiste tú, haz clic en el botón de abajo para elegir una nueva:</p>
                                
                                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin: 30px 0;'>
                                    <tr>
                                        <td align='center'>
                                            <a href='{resetUrl}' style='display: inline-block; background-color: #FF7A59; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 16px; padding: 14px 32px; border-radius: 12px;'>
                                                Nueva contraseña
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                                
                                <p style='margin: 0 0 20px 0; font-size: 14px; color: #888888;'>Este enlace caducará en 1 hora por motivos de seguridad.</p>
                                <p style='margin: 0; font-size: 14px; color: #888888;'>Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña actual seguirá siendo válida.</p>
                            </td>
                        </tr>
                    </table>
                    
                    <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; margin-top: 20px;'>
                        <tr>
                            <td align='center' style='color: #999999; font-size: 12px; padding: 20px;'>
                                Enviado con ❤️ por <strong>Zezame</strong><br>
                                © {DateTime.Now.Year} Zezame. Todos los derechos reservados.
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }
    }
}
