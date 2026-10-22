using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:Default"]
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'Default'. Defínela como variable de entorno.");

builder.Services.AddDbContext<ZezameDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITenantContext, DesignTimeTenantContext>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<ZezameDbContext>();

Console.WriteLine("Aplicando migraciones pendientes...");
await db.Database.MigrateAsync();
Console.WriteLine("Migraciones aplicadas correctamente.");