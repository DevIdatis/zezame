using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Zezame.Api.Email;
using Zezame.Api.Hubs;
using Zezame.Api.Middleware;
using Zezame.Api.Multitenancy;
using Zezame.Api.Services;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;
using Zezame.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:4200";

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Falta la configuración 'Jwt:Key'. Defínela en user-secrets o en una variable de entorno.");

var resendKey = builder.Configuration["RESEND_APITOKEN"]
    ?? throw new InvalidOperationException(
        "Falta la configuración 'RESEND_APITOKEN'. Defínela en user-secrets o en una variable de entorno.");

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", p => p
    .WithOrigins(frontendUrl) // Origen espec�fico, no wildcard
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()));

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddDbContext<ZezameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => {
        opts.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };
        // Configurar JWT para SignalR
        opts.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                var accessToken = context.Request.Query["access_token"];

                // Si el request es para SignalR hub
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/timeHub")) {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context => {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning(context.Exception,
                    "JWT RECHAZADO: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context => {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning(
                    "JWT CHALLENGE: error={Error} desc={Description}",
                    context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options => {
    // Managers, Admins y Owners pueden gestionar proyectos/actividades/equipo.
    options.AddPolicy("RequireManager", policy =>
        policy.RequireRole("Manager", "Admin", "Owner"));
    // Admins y Owners: administración de la organización.
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin", "Owner"));
});

// Rate limiting: limita los intentos en endpoints de autenticación por IP
// para mitigar fuerza bruta y abuso. Política "auth" aplicada en AuthController.
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddSignalR();

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o => {
    o.ApiToken = resendKey;
});
builder.Services.AddTransient<IResend, ResendClient>();

// Repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

// Contexto de tenant (organización de la petición), consumido por el query filter de EF.
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Envío de email: implementación de desarrollo (registra en log). En producción,
// sustituir por un IEmailSender real (SMTP/proveedor) sin tocar la lógica.
builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();

// Servicios
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ITokenIssuer>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<InvitationService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<TimeService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// El middleware de excepciones va primero para envolver toda la tubería.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Resolución de tenant: tras autenticar, fija la organización de la petición.
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseRateLimiter();

app.MapHub<TimeHub>("/timeHub");
app.MapControllers();

app.Run();
