using GameServer.DTO.Game;

namespace GameServer.Services.Game;

public interface IGameService
{
    Task<GameDto.CreateGameResponse> CreateLobby(GameDto.CreateGameRequest createGameRequest);
    Task<GameDto.JoinGameResponse> JoinGame(GameDto.JoinGameRequest joinGameRequest);
    Task<GameDto.IsLoadedResponse> LoadGame(GameDto.IsLoadedRequest isLoadedRequest);

}