using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using inmobiliariaApi.Data;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.Services;

// 🚀 Forzamos que Kestrel use solo el puerto 2000 en todas las interfaces
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://0.0.0.0:2000");


var builder = WebApplication.CreateBuilder(args); // aca lo q se hace es crear la instancia de la app

builder.Logging.ClearProviders(); // esto es para limpiar los proveedores de logging
builder.Logging.AddConsole(); // esto es para agregar el proveedor de logging de consola

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // solo warnings+ comandos SQL
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Error); // oculta warnings de query (como Skip/Take sin OrderBy)
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error); // para otros logs EF Core, solo errores
builder.Logging.AddFilter("Microsoft", LogLevel.Warning); // Microsoft en general, solo warnings+
builder.Logging.AddFilter("System", LogLevel.Warning); // System en general, solo warnings+

builder.Logging.SetMinimumLevel(LogLevel.Information); // para tu código, info+


//Cambio
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200", // Desarrollo
                    "http://localhost:4200/", // Desarrollo
                    "https://test.buscopropiedades.com.ar", // Producción
                    "http://test.buscopropiedades.com.ar", // Producción
                "http://buscopropiedades.com.ar", // Producción
                "https://buscopropiedades.com.ar" // Producción
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

// Agregar controladores y explorador de API
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

//Repositorios
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<LeadRepository>();

//Services
builder.Services.AddScoped(typeof(GenericService<>));
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<IUsoMensualService, UsoMensualService>();

// Tenant Context - Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Configurar Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Introduce tu token JWT aquí. Ejemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    // options.IncludeXmlComments(xmlPath);
});

// Configurar base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MariaDbServerVersion(new Version(10, 4, 32))
    )
);

// Configurar autenticación JWT
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // esto es para autenticar con JWT
    .AddJwtBearer(options => // esto es para configurar JWT
    {
        var jwtKey =
            builder.Configuration["Jwt:Key"]
            ?? throw new ArgumentNullException("Jwt:Key no está definido"); // esto es para obtener la clave de JWT

        var key = Encoding.ASCII.GetBytes(jwtKey); // esto es para convertir la clave de JWT a bytes

        // esto es para configurar JWT para autenticación y autorización de JWT
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

builder.Services.AddAuthorization(); // esto lo q hace es configurar autorización

// Registrar servicios de negocio
builder.Services.AddSingleton(builder.Configuration);

// Registrar el background service para alertas de herramientas
// builder.Services.AddHostedService<HerramientaAlertBackgroundService>();

// Registrar servicios de acceso a datos
var app = builder.Build();

// Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Aplica la política de CORS después de autenticación pero antes de autorización
app.UseCors("AllowFrontend");

//esto va a servir el contenido estatico desde la carpeta wwwroot
app.UseStaticFiles();


app.UseAuthentication();


app.UseAuthorization();

// Rutas de ejemplo
var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapControllers(); // esto es para habilitar los enroutadores de los controladores

//Original
// app.Run();
//Cambio
// app.Run("http://0.0.0.0:1000");
// Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://0.0.0.0:1000");

app.Run(); // inicia la aplicación


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
