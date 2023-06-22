using GameServer.Postgres;
using Microsoft.EntityFrameworkCore;
using GameServer;
using GameServer.Endpoints;
using GameServer.Services.Auth;
using GameServer.Services.Game;
using GameServer.Services.SignalR;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc();
builder.Services.AddSignalR();

builder.Services.AddSingleton(new SocketServerHub());
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql());
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(AuthEndpoints.ApiError), () => new OpenApiSchema { /* настройки схемы для ApiError */ });
});


var app = builder.Build();


// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseRouting();

app.MapHub<SocketServerHub>("/socket");


app.UseAuthentication();
app.UseAuthorization();

app.ConfigureAuthEndpoints();
app.ConfigureLobbyEndpoints();

app.UseCors(corsBuilder => corsBuilder.WithOrigins("http://localhost:5173", "http://localhost:5174").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
app.Run("http://0.0.0.0:5157");