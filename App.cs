using GameServer.Postgres;
using Microsoft.EntityFrameworkCore;
using GameServer;
using GameServer.Endpoints;
using GameServer.Services.Auth;
using GameServer.Services.Game;
using GameServer.SocketServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddControllers();
builder.Services.AddSingleton<SocketServer>(new SocketServer());
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql());
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();

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
app.ConfigureLobbyEndpoints();

app.UseHttpsRedirection();
app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.Run("http://0.0.0.0:5157");