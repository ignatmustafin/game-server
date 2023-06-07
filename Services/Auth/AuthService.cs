using GameServer.DTO.Auth;
using GameServer.Models;
using GameServer.Postgres;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthDto.SignUpResponse> SignUp(AuthDto.SignUpRequest signUpRequest)
    {
        User player = new User()
        {
            Name = signUpRequest.Name,
            Email = signUpRequest.Email,
            Password = signUpRequest.Password
        };

        bool uniquePlayer = await IsUniqueEmail(player.Email);
        if (!uniquePlayer)
            throw new Exception("Player with such email is already exist");

        
        var createdPlayer = await _db.User.AddAsync(player);
        await _db.SaveChangesAsync();
        AuthDto.SignUpResponse data = new AuthDto.SignUpResponse(createdPlayer.Entity.Id, createdPlayer.Entity.Email, createdPlayer.Entity.Name);
        return data;
    }
    
    public async Task<AuthDto.SignInResponse> SignIn(AuthDto.SignInRequest signInRequest)
    {
        User player = new User()
        {
            Email = signInRequest.Email,
            Password = signInRequest.Password
        };
        
        User foundedPlayer = await _db.User.FirstOrDefaultAsync(p => p.Email == player.Email);
        if (foundedPlayer == null || signInRequest.Password != foundedPlayer.Password)
            throw new Exception("Incorrect email or password");
        AuthDto.SignInResponse data = new AuthDto.SignInResponse(foundedPlayer.Id, foundedPlayer.Email, foundedPlayer.Name);
        return data;
    }

    public async Task<bool> IsUniqueEmail(string email)
    {
        User playerExist = await _db.User.FirstOrDefaultAsync(p => p.Email == email);

        return playerExist == null;
    }
}