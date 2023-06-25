using System.ComponentModel.DataAnnotations;
using GameServer.DTO.Game;
using Newtonsoft.Json;

namespace GameServer.Models;

public enum CardIn
{
    Hand,
    Field1,
    Field2,
    Field3,
    Field4,
}

public enum SideState
{
    Back,
    Front
}

public class PlayerCard : GameDto.CardBase
{
    [JsonProperty("id")] public int Id { get; set; }
    [Required]
    [JsonProperty("playerId")] public int PlayerId { get; set; }
    [JsonIgnore] [JsonProperty("player")] public Player Player { get; set; }
    [Required]
    [JsonProperty("cardId")] public int CardId { get; set; }
    [JsonIgnore] [JsonProperty("card")] public Card Card { get; set; }
    [Required] 
    [EnumDataType(typeof(CardIn))]
    [JsonProperty("cardIn")] public CardIn CardIn { get; set; }

    [JsonProperty("isDead")] public bool IsDead { get; set; }
    [JsonProperty("manacost")] public int Manacost { get; set; }
    [JsonProperty("hp")] public int Hp { get; set; }
    [JsonProperty("damage")] public int Damage { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("type")] [EnumDataType(typeof(CardType))] public CardType Type { get; set; }
    [JsonProperty("sideState")] [EnumDataType(typeof(SideState))] public SideState SideState { get; set; } = SideState.Back;
    [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
}