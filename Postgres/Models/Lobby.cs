using System.Text.Json.Serialization;

namespace GameServer.Postgres.Models;

public class Lobby
{
    [JsonPropertyName("Id")]
    public Guid Id { get; set; }
    [JsonPropertyName("JoinedUsers")]
    public int JoinedUsers { get; set; }
    [JsonPropertyName("LoadedUsers")]
    public int LoadedUsers { get; set; }
    [JsonPropertyName("EndedTurnUsers")]
    public int EndedTurnUsers { get; set; }
    [JsonPropertyName("TimerLeft")]
    public int TimerLeft { get; set; }
}