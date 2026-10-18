namespace Zezame.Api.Email {
    /// <summary>
    /// Envío de email. En desarrollo se registra en el log; en producción se
    /// enchufa una implementación SMTP/proveedor sin tocar la lógica de negocio.
    /// </summary>
    public interface IEmailSender {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
