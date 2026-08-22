using System.Text;
using FCG.Api.Middleware;
using FCG.Api.Seeding;
using FCG.Api.Services;
using FCG.Application.Abstractions;
using FCG.Application.Games.CreateGame;
using FCG.Application.Libraries.AddLibraryItem;
using FCG.Application.Promotions.CreatePromotion;
using FCG.Application.Users.RegisterUser;
using FCG.Infrastructure;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Infraestrutura (DB + repositórios + UnitOfWork) ---
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration.GetConnectionString("Fcg")
    ?? throw new InvalidOperationException("Connection string não configurada.");
builder.Services.AddInfrastructure(connectionString);

// --- Serviços de segurança e infra transversal ---
builder.Services.AddSingleton<BCryptPasswordHasher>();
builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasherAdapter>();
builder.Services.AddSingleton<IClock, SystemClock>();

// --- Application handlers ---
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<CreateGameHandler>();
builder.Services.AddScoped<CreatePromotionHandler>();
builder.Services.AddScoped<AddLibraryItemHandler>();

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

builder.Services.AddAuthorization();

// --- Controllers e Swagger ---
builder.Services.AddControllers();
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
