namespace Zezame.Api.Email {
    /// <summary>
    /// Implementación de desarrollo: no envía nada, escribe el email (destinatario,
    /// asunto y cuerpo con el enlace/token) en el log. Permite probar los flujos de
    /// verificación y reset en local sin un proveedor de correo.
    /// </summary>
    public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender {
        private readonly ILogger<LoggingEmailSender> _logger = logger;

        public Task SendAsync(string toEmail, string subject, string body) {
            _logger.LogInformation(
                "[EMAIL-DEV] Para: {To}\nAsunto: {Subject}\n{Body}",
                toEmail, subject, body);
            return Task.CompletedTask;
        }
    }
}
