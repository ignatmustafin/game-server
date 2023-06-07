namespace GameServer.DTO.Game;

public class GameDto
{
    public record CreateGameRequest(int UserId);
    public record CreateGameResponse(Guid Link, int PlayerId);
    
    public record JoinGameRequest(Guid Link, int UserId);
    public record JoinGameResponse(int PlayerId);
    
    public record IsLoadedRequest(int PlayerId);
    public record IsLoadedResponse(bool Success = true);
    
    
}