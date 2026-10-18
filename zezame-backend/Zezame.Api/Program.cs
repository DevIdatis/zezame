using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Zezame.Api.Hubs;
using Zezame.Api.Middleware;
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
            ValidateLifetime = true,
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
            }
        };
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireCoordinator", policy =>
        policy.RequireRole("Coordinator", "Admin"));
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
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

// Repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

// Servicios
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<TimeService>();
builder.Services.AddScoped<ReportService>();

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
app.UseRateLimiter();

app.MapHub<TimeHub>("/timeHub");
app.MapControllers();

app.Run();
