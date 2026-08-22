using FCG.Api.Services;
using FCG.Application.Abstractions;
using FCG.Application.Games.CreateGame;
using FCG.Application.Libraries.AddLibraryItem;
using FCG.Application.Promotions.CreatePromotion;
using FCG.Application.Users.RegisterUser;
using FCG.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Fcg") ?? "Host=localhost;Database=fcg;Username=fcg;Password=fcg_local_password");
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<RegisterUserHandler>(); builder.Services.AddScoped<CreateGameHandler>(); builder.Services.AddScoped<CreatePromotionHandler>(); builder.Services.AddScoped<AddLibraryItemHandler>();
var app = builder.Build(); app.MapControllers(); app.Run();
public partial class Program { }
