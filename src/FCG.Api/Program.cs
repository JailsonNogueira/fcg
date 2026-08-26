using System.Text;
using System.Text.Json.Serialization;
using FCG.Api.Authorization.DependencyInjection;
using FCG.Api.DependencyInjection;
using FCG.Api.Middleware;
using FCG.Api.Seeding;
using FCG.Api.Services;
using FCG.Application.Abstractions;
using FCG.Infrastructure;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "logs/fcg-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, cfg) => cfg
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "logs/fcg-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

// --- Infraestrutura (DB + repositórios + UnitOfWork) ---
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration.GetConnectionString("Fcg")
    ?? throw new InvalidOperationException("Connection string não configurada.");
builder.Services.AddInfrastructure(connectionString);

// --- Serviços de segurança e infra transversal ---
builder.Services.AddSingleton<BCryptPasswordHasher>();
builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddSingleton<ITokenGenerator>(sp => sp.GetRequiredService<JwtTokenGenerator>());
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasherAdapter>();
builder.Services.AddSingleton<IClock, SystemClock>();

// --- Application handlers ---
builder.Services.AddApplicationHandlers();

// --- Autenticação JWT ---
var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddFcgAuthorization();

// --- Controllers e Swagger ---
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Sem isso, "role": "Administrator" só seria aceito como número no corpo da requisição.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FIAP Cloud Games API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido no endpoint /auth/login.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// --- Admin Seeder ---
builder.Services.AddHostedService<AdminSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
