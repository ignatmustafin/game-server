using System.ComponentModel;
using System.Text.Json.Serialization;

namespace GameServer.Models;

public class Player
{
    [JsonPropertyName("Id")] public int Id { get; set; }

    [JsonPropertyName("GameId")] public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    [JsonPropertyName("UserId")] public int UserId { get; set; }
    public User User { get; set; } = null!;

    [JsonPropertyName("IsLoaded")] public bool IsLoaded { get; set; }

    [JsonPropertyName("Hp")] public int Hp { get; set; } = 30;

    [JsonPropertyName("Mana")] public int Mana { get; set; } = 1;
    
    public ICollection<PlayerCard> CardsInHand { get; } = new List<PlayerCard>();
}