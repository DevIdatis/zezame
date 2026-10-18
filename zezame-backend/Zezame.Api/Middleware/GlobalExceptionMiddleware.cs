using System.Net;
using System.Text.Json;
using Zezame.Api.Exceptions;

namespace Zezame.Api.Middleware {
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger) {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context) {
            try {
                await _next(context);
            } catch (Exception ex) {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception) {
            var statusCode = exception switch {
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ForbiddenException => (int)HttpStatusCode.Forbidden,
                NotFoundException => (int)HttpStatusCode.NotFound,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var traceId = context.TraceIdentifier;

            // Los errores 5xx se registran completos (con stack trace) pero NUNCA se
            // devuelven al cliente: solo un mensaje genérico y el traceId para soporte.
            // El resto son errores esperados y de negocio: su mensaje es seguro de mostrar.
            string clientMessage;
            if (statusCode >= 500) {
                _logger.LogError(exception,
                    "Error no controlado ({TraceId}) en {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);
                clientMessage = "Se ha producido un error interno. Si persiste, contacta con soporte indicando el identificador.";
            } else {
                _logger.LogWarning(
                    "Petición rechazada ({TraceId}) {StatusCode} en {Method} {Path}: {Message}",
                    traceId, statusCode, context.Request.Method, context.Request.Path, exception.Message);
                clientMessage = exception.Message;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new {
                statusCode,
                message = clientMessage,
                traceId
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
