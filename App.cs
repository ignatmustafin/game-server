using GameServer.Postgres;
using Microsoft.EntityFrameworkCore;
using GameServer;
using GameServer.Endpoints;
using GameServer.Services.Auth;
using GameServer.SocketServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddControllers();
builder.Services.AddSingleton<SocketServer>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=game;Username=macbookair;Password=admin"));
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseAuthentication();
app.UseAuthorization();

app.ConfigureAuthEndpoints();
app.UseHttpsRedirection();
app.Run("http://0.0.0.0:5157");