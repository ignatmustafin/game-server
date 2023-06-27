using GameServer.DTO.Game;

namespace GameServer.Services.Game;

public interface IGameService
{
    Task<GameDto.CreateGameResponse> CreateLobby(GameDto.CreateGameRequest createGameRequest);
    Task<GameDto.JoinGameResponse> JoinGame(GameDto.JoinGameRequest joinGameRequest);
    Task<GameDto.FindGameResponse> FindGame(GameDto.FindGameRequest findGameRequest);
    Task<GameDto.IsLoadedResponse> LoadGame(GameDto.IsLoadedRequest isLoadedRequest);
    Task<GameDto.CardThrownResponse> CardThrown(GameDto.CardThrownRequest cardThrownRequest);
    Task<GameDto.EndTurnResponse> EndTurn(GameDto.EndTurnRequest endTurnRequest);
}