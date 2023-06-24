using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GameServer.Models;
using Newtonsoft.Json;

namespace GameServer.DTO.Game;

public class GameDto
{
     public record CreateGameRequest(int UserId);
     public record CreateGameResponse(Guid Link, int PlayerId, int GameId);
    
    public record JoinGameRequest(Guid Link, int UserId);
    public record JoinGameResponse(int PlayerId, int GameId);
    
    public record IsLoadedRequest(int PlayerId, int GameId);
    public record IsLoadedResponse(bool Success = true);

    public record CardThrownRequest(int PlayerId, int CardId, [property: System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))] CardIn Field);
    public record CardThrownResponse(bool Success = true);

    public record EndTurnRequest(int PlayerId);
    public record EndTurnResponse(bool Success = true);
    
    public class CardBase
    {
    }
    
    public class PlayerData
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("hp")] public int Hp { get; set; }
        [JsonProperty("manaCommon")] public int ManaCommon { get; set; }
        [JsonProperty("manaCurrent")] public int ManaCurrent { get; set; }
        [JsonProperty("cardsInHand")] public ICollection<PlayerCard> CardsInHand { get; set; }
        [JsonProperty("field1")] public PlayerCard Field1 { get; set; }
        [JsonProperty("field2")] public PlayerCard Field2 { get; set; }
        [JsonProperty("field3")] public PlayerCard Field3 { get; set; }
        [JsonProperty("field4")] public PlayerCard Field4 { get; set; }
    }

    public class EnemyData
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("hp")] public int Hp { get; set; }
        [JsonProperty("manaCommon")] public int ManaCommon { get; set; }
        [JsonProperty("cardsInHand")] public ICollection<EnemyCardType> CardsInHand { get; set; }
        [JsonProperty("field1")] public CardBase Field1 { get; set; }
        [JsonProperty("field2")] public CardBase Field2 { get; set; }
        [JsonProperty("field3")] public CardBase Field3 { get; set; }
        [JsonProperty("field4")] public CardBase Field4 { get; set; }
    }

    public class EnemyCardType : CardBase
    {
        [JsonProperty("type")] [EnumDataType(typeof(CardType))] public CardType Type { get; set; }
    }
    
    public record GameData([property: JsonProperty("playerData")] PlayerData PlayerData, [property: JsonProperty("enemyData")] EnemyData EnemyData);
    
    public record CardAttack([property: JsonProperty("field")] CardIn Field,
        [property: JsonProperty("attackingPlayer")] Player AttackingPlayer,
        [property: JsonProperty("attackingCard")] PlayerCard AttackingCard,
        [property: JsonProperty("playerUnderAttack")] Player PlayerUnderAttack,
        [property: JsonProperty("fieldsUnderAttack")] ICollection<PlayerCard>
            FieldsUnderAttack);

    public record CardIsDead([property: JsonProperty("field")] CardIn Field,
        [property: JsonProperty("playerUnderAttack")] Player PlayerUnderAttack);
    
}