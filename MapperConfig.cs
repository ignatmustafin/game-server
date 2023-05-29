using AutoMapper;
using GameServer.DTO.Auth;
using GameServer.Models;

namespace GameServer;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        // Plauer
        CreateMap<Player, AuthDto.SignUpResponse>();
        CreateMap<AuthDto.SignUpRequest, Player>();
    }
}