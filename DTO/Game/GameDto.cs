using System.Text.Json.Serialization;
using GameServer.Models;

namespace GameServer.DTO.Game;

public class GameDto
{
    public record CreateGameRequest(int UserId);
    public record CreateGameResponse(Guid Link, int PlayerId, int GameId);
    
    public record JoinGameRequest(Guid Link, int UserId);
    public record JoinGameResponse(int PlayerId, int GameId);
    
    public record IsLoadedRequest(int PlayerId, int GameId);
    public record IsLoadedResponse(bool Success = true);

    public record CardThrownRequest(int PlayerId, int CardId, [property: JsonConverter(typeof(JsonStringEnumConverter))] CardIn Field);
    public record CardThrownResponse(bool Success = true);

    public record EndTurnRequest(int PlayerId);
    public record EndTurnResponse(bool Success = true);
    
    public class PlayerData
    {
        public string Name { get; set; }
        public int Hp { get; set; }
        public int Mana { get; set; }
        public ICollection<PlayerCard> CardsInHand { get; set; }
        public PlayerCard Field1 { get; set; }
        public PlayerCard Field2 { get; set; }
        public PlayerCard Field3 { get; set; }
        public PlayerCard Field4 { get; set; }
    }

    public class EnemyData
    {
        public string Name { get; set; }
        public int Hp { get; set; }
        public int Mana { get; set; }
        public int CardsInHandCount { get; set; }
        public PlayerCard Field1 { get; set; }
        public PlayerCard Field2 { get; set; }
        public PlayerCard Field3 { get; set; }
        public PlayerCard Field4 { get; set; }
    }
    
    public record GameData(PlayerData PlayerData, EnemyData EnemyData);

}